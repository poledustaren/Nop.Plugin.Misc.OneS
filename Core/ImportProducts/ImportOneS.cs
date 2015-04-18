using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Nop.Plugin.Misc.OneS.Models;
using Nop.Services.Logging;

namespace Nop.Plugin.Misc.OneS.Core.ImportProducts
{
    public class ImportOneS : IImportOneS
    {
        
        private readonly Regex RemoveWhitespaceRegEx = new Regex(@"\s+", RegexOptions.Compiled);
        
        private readonly IImportOneSImpl _importOneSImpl;
        private readonly ILogger _logger;
        private ConfigImportCategoryEntity _configImportCategoryEntity;

        public ImportOneS(IImportOneSImpl importOneSImpl, ILogger _logger)
        {
            _importOneSImpl = importOneSImpl;
            this._logger = _logger;
            _configImportCategoryEntity=new ConfigImportCategoryEntity();
        }

        public bool ImportProductStorages(string path,ConfigImportCategoryEntity config)
        {
            var xmlRootElement = GetXmlRootElement(path,config.RootCategoryName);
            _configImportCategoryEntity = config;
            _importOneSImpl.SetConfig(config);

            if (xmlRootElement == config.RootName.ToLower())
            {
                var importEntities = ParseXml(path, config);
                _importOneSImpl.ImportStoragesEntities(importEntities);
                return true;
            }
            if (xmlRootElement  == "products")
            {
                var importEntities = ParseXml(path, config);
                _importOneSImpl.ImportAllEntities(importEntities);
                return true;
            }
            return false;
            
        }

        private ImportEntity[] ParseXml(string path,ConfigImportCategoryEntity config)
        {
            var root = OpenXml(path);
            var importEntities=new List<ImportEntity>();

            importEntities.AddRange(SubCategoryElements(root));
            return importEntities.ToArray();
        }

        private IEnumerable<ImportEntity> SubCategoryElements(XElement root)
        {
            var rootCategory = root.Element(_configImportCategoryEntity.RootCategoryName);
            if (rootCategory != null)
            {
                var rootCategoryElements = rootCategory.Elements(_configImportCategoryEntity.RootCategoryElementsName);

                var range = ParseSubCategoryElements(rootCategoryElements);
                return range;
            }
           return new List<ImportEntity>();
        }

        private IEnumerable<ImportEntity> ParseSubCategoryElements(IEnumerable<XElement> elements)
        {
                var importEntities = new List<ImportEntity>();
                foreach (var element in elements)
                {
                    var brandSku = (string)element.Attribute("Brand.Id");
                    var brandName = (string)element.Attribute("Brand");
                    var modelSku = (string)element.Attribute("Id");
                    var modelName = (string)element.Attribute("Title");
                    try
                    {
                        var modelRoot = element.Element("Models");
                        if (modelRoot == null)
                            throw new Exception("No models in element");

                        var models = modelRoot.Elements("Model");

                        foreach (var model in models)
                        {
                            var importEntity = ParseModel(brandSku, brandName, modelSku, modelName, model);
                            importEntities.Add(importEntity);
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.Error(modelSku + " " + modelName, e);
                        throw;
                    }
                }
                return importEntities;
        }

        private ImportEntity ParseModel(string brandSku, string brandName, string modelSku, string modelName, XElement model)
        {
            var productSpecifications = _configImportCategoryEntity.ProductSpecifications.Select(i => GetProductSpecification(model, i.XName, i.RusName)).ToList();

            var productName = (string) model.Attribute("Title");
            var productSku = (string)model.Attribute("Id");

            //проверка на удаление
            var delete = model.Attribute("Delete").Value;
            var isDeleted = delete.Equals("true");

            //Добавляем опцию спецификации для производителя
            if (brandName != "КРЕПЕЖ")
            {
                var brandSpecification = new ProductSpecification()
                {
                    Name = "Производитель",
                    Value = brandName
                };
                productSpecifications.Add(brandSpecification);
            }
            
            var storageRoot = model.Element("Storages");
            if (storageRoot == null)
                throw new Exception("No storages root");
            var storages = storageRoot.Elements("Storage");
            if (storages == null)
                throw new Exception("No storages");

            var importEntity = new ImportEntity
            {
                BrandName = brandName,
                BrandSku = brandSku,
                ModelSku = modelSku,
                ModelName = modelName,
                ProductName = brandName + " " + productName,
                IsDeleted = isDeleted,
                ProductSku = productSku,
                Storages = storages.Select(GetProductStorage).ToArray(),
                ProductSpecifications = productSpecifications.ToArray(),
                CategoryId = _configImportCategoryEntity.CategoryId
            };
            return importEntity;
        }


        private ProductSpecification GetProductSpecification(XElement model, string specificationKey, string specificationValue)
        {
            var specValue = (string) model.Attribute(specificationKey);
           
            return new ProductSpecification
            {
                Value = specValue,
                Name = specificationValue
            };
        }

        private ProductStorage GetProductStorage(XElement storage)
        {
            var dirtyPrice = (string)storage.Attribute("Price");
            var nicePrice = NicePrice(dirtyPrice);

            var dirtyQuantity = (string)storage.Attribute("Quantity");
            int quantity = ParseQuanity(dirtyQuantity);

            var name = (string) storage.Attribute("Storage");
            return new ProductStorage
            {
                Name = name,
                Price = nicePrice,
                Quantity = quantity
            };
        }

        private decimal NicePrice(string notNicePrice)
        {
            var niceStringPrice = RemoveWhitespaceRegEx.Replace(notNicePrice, "").Replace(",",".");
            if (string.IsNullOrEmpty(niceStringPrice))
                return 0;
            decimal nicePrice;

            if (decimal.TryParse(niceStringPrice, NumberStyles.Currency, CultureInfo.InvariantCulture, out nicePrice))
                return nicePrice;
            _logger.Error("Не удалось обработать цену " + notNicePrice);
            return nicePrice;
        }

        private int ParseQuanity(string dirtyQuanity)
        {
            var niceQuanityString = RemoveWhitespaceRegEx.Replace(dirtyQuanity, "");
            if (string.IsNullOrEmpty(niceQuanityString))
                return 0;
            int nicePrice;

            if (int.TryParse(niceQuanityString, NumberStyles.Currency, CultureInfo.InvariantCulture, out nicePrice))
                return nicePrice;
            _logger.Error("Не удалось обработать цену " + dirtyQuanity);
            return nicePrice;
        }

        private XElement OpenXml(string path)
        {
            return XElement.Load(path);
        }

        private string GetXmlRootElement(string path, string rootCategoryName)
        {
            var root = OpenXml(path);
            var fileRoot = root.Element(rootCategoryName);
            if (fileRoot == null)
                return null;
            return root.Name.ToString().ToLower();
        }


    }
}