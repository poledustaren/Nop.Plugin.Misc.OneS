using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;

using Nop.Plugin.Misc.OneS.Models;
using Nop.Services.Catalog;
using Nop.Services.Seo;

namespace Nop.Plugin.Misc.OneS.Core.ImportProducts
{
    public class ImportOneSImpl : IImportOneSImpl
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IRepository<Product> _productRepository;
        private readonly MiscOneSSettings _miscOneSSetting;
        private ConfigImportCategoryEntity _configImportCategoryEntity;

        public ImportOneSImpl(
             IProductService productService,
            ICategoryService categoryService,
            ISpecificationAttributeService specificationAttribute,
            IProductAttributeService productAttributeService,
            IUrlRecordService urlRecordService,
            IRepository<Product> producRepository,
            MiscOneSSettings miscOneSSetting)
        {
            _productService = productService;
            _categoryService = categoryService;
            _specificationAttributeService = specificationAttribute;
            _productAttributeService = productAttributeService;
            _urlRecordService = urlRecordService;
            _productRepository = producRepository;
            _miscOneSSetting = miscOneSSetting;
            _configImportCategoryEntity=new ConfigImportCategoryEntity();
        }

        public void SetConfig(ConfigImportCategoryEntity configImportCategoryEntity)
        {
            _configImportCategoryEntity = configImportCategoryEntity;
        }



        public void ImportAllEntities(ImportEntity[] importEntities)
        {
            var allProducts = GetAllLastImportProducts(_configImportCategoryEntity.CategoryId);
            
            var allPublishedProducts = GetAllPublishedProducts(_configImportCategoryEntity.CategoryId);
            var allProductNameFromXml = importEntities.Select(x => x.ProductName).ToList();


           
          
            ImportEntites(importEntities);
            UpdateProductsPublish(allPublishedProducts, allProductNameFromXml);
            UpdateLastStatus(allProducts, allProductNameFromXml, importEntities);
        }

        private void UpdateLastStatus(IEnumerable<Product> allProducts, List<string> allProductNameFromXml, ImportEntity[] importEntities)
        {
            //лишим статуса последнего те, которые не были последними в импорте
            foreach (var product in allProducts)
            {
                var checkProductFromBaseInXml = allProductNameFromXml.FirstOrDefault(x => x.Equals(product.Name));

                if (checkProductFromBaseInXml != null) continue;
                product.AdminComment = null;
                _productService.UpdateProduct(product);
            }

            foreach (var productName in allProductNameFromXml)
            {
                var firstOrDefault = importEntities.FirstOrDefault(x => x.ProductName.Equals(productName));
                var sku = "";
                if (firstOrDefault == null) continue;
                sku = firstOrDefault.ProductSku;
                var productFromBase = _productService.GetProductBySku(sku);
                if (productFromBase.AdminComment != "last")
                {
                    productFromBase.AdminComment = "last";
                    _productService.UpdateProduct(productFromBase);
                }
               
            }
            
        }


        public void ImportStoragesEntities(ImportEntity[] importEntities)
        {
            ImportEntites(importEntities);
        }

        void ImportEntites(ImportEntity[] importEntities)
        {
            foreach (var importEntity in importEntities)
            {
                if (importEntity.IsDeleted)
                {
                    var product = _productService.GetProductBySku(importEntity.ProductSku);
                    if (product!=null)
                    {
                        _productService.DeleteProduct(product);
                    }
                }
                else
                {
                    UpdateBrands(importEntity);
                    UpdateModels(importEntity);
                    UpdateProduct(importEntity);
                    UpdateSpecificationOption(importEntity);
                    UpdateProductSpecificationOption(importEntity);
                    CheckProductStoragesPriceAndQuantites(importEntity);
                }
            }
        }

        private void UpdateProductsPublish(IEnumerable<Product> allProductDisplayedOnHomePage, List<string> allProductFromXml)
        {
            foreach (var product in allProductDisplayedOnHomePage)
            {
                var checkProductFromBaseInXml = allProductFromXml.FirstOrDefault(x => x.Equals(product.Name));              
                if (checkProductFromBaseInXml != null) continue;
                product.Published = false;
                _productService.UpdateProduct(product);
            }
              
        }

        private void CheckProductStoragesPriceAndQuantites(ImportEntity importEntity)
        {
            var product = _productService.GetProductBySku(importEntity.ProductSku);
            var productVariantGet = _productAttributeService.GetProductVariantAttributesByProductId(product.Id);
            //связываем продукт и свойство(склад)
            if (!productVariantGet.Any())
            {
                var productVariantAttribute = new ProductVariantAttribute();
                productVariantAttribute.ProductId = product.Id;
                productVariantAttribute.ProductAttributeId = 1; //TODO:другой способ ввода id
                productVariantAttribute.AttributeControlTypeId = 1;
                _productAttributeService.InsertProductVariantAttribute(productVariantAttribute);
            }

            var needPreselected = true;
            foreach (var entity in importEntity.Storages)
            {
                //связываем названия свойств с маппингом продуктов и свойств
                var productVariantGetForValue =
                    _productAttributeService.GetProductVariantAttributesByProductId(product.Id).FirstOrDefault();
                var productVariantValueGet =
                    _productAttributeService.GetProductVariantAttributeValues(productVariantGetForValue.Id);
                var productVariantAttr = productVariantValueGet.FirstOrDefault(x => x.Name.Equals(entity.Name));

                var isPreselected = PreSelectedUpdate(entity, ref needPreselected);
                //цена для отображения
                if (isPreselected)
                {
                    product.Price = entity.Price;
                    _productService.UpdateProduct(product);
                }
                if (productVariantAttr == null)
                {
                    var productVariantAttributeValue = new ProductVariantAttributeValue();
                    var productVariantAttributeForMap =
                        _productAttributeService.GetProductVariantAttributesByProductId(product.Id)
                            .FirstOrDefault();
                    productVariantAttributeValue.ProductVariantAttributeId =
                        productVariantAttributeForMap != null
                            ? productVariantAttributeForMap.Id
                            : new int();
                    productVariantAttributeValue.Name = entity.Name;
                    productVariantAttributeValue.IsPreSelected =isPreselected;
                    
                    _productAttributeService.InsertProductVariantAttributeValue(
                        productVariantAttributeValue);
                }
                else
                {
                    productVariantAttr.IsPreSelected = PreSelectedUpdate(entity, ref needPreselected);
                    _productAttributeService.UpdateProductVariantAttributeValue(productVariantAttr);
                }
               


                //связываем продукт и значения свойств
                var productVariantAttributeCombinationsGet =
                    _productAttributeService.GetAllProductVariantAttributeCombinations(product.Id);

                var productVariantAttributeValueForXml=_productService.GetProductBySku(importEntity.ProductSku)
                    .ProductVariantAttributes.FirstOrDefault()
                    .ProductVariantAttributeValues.FirstOrDefault(x => x.Name.Equals(entity.Name));
                var productVariantAttributeCombination = productVariantAttributeCombinationsGet.FirstOrDefault(
                         x => x.Sku.Equals(entity.Name));

                if (productVariantAttributeCombination==null)
                {
                    productVariantAttributeCombination = new ProductVariantAttributeCombination();
                    var productXml = "<Attributes><ProductVariantAttribute ID='" +
                                     productVariantAttributeValueForXml.ProductVariantAttributeId +
                                     "'><ProductVariantAttributeValue><Value>" +
                                     productVariantAttributeValueForXml.Id +
                                     "</Value></ProductVariantAttributeValue></ProductVariantAttribute></Attributes>";

                    productVariantAttributeCombination.ProductId = product.Id;
                    productVariantAttributeCombination.AttributesXml = productXml;
                    productVariantAttributeCombination.StockQuantity = entity.Quantity;
                    productVariantAttributeCombination.OverriddenPrice = entity.Price;
                    productVariantAttributeCombination.Sku = entity.Name;
                    
                    _productAttributeService.InsertProductVariantAttributeCombination(
                        productVariantAttributeCombination);
                }
                else
                {
                    
                    
                    productVariantAttributeCombination.StockQuantity = entity.Quantity;
                    productVariantAttributeCombination.OverriddenPrice = entity.Price;
                    _productAttributeService.UpdateProductVariantAttributeCombination(productVariantAttributeCombination);
                }
            }
        }

        private bool PreSelectedUpdate(ProductStorage entity, ref bool weNeedPreselected)
        {

            if (_miscOneSSetting.PublishOnlyBerikolesaStorage)
            {
                return String.Equals(entity.Name, "Berikolesa.RU", StringComparison.CurrentCultureIgnoreCase);
            }
            
            if (entity.Quantity <= 0 || !weNeedPreselected) return false;
            
            weNeedPreselected = false;
            return true;
        }

        private void UpdateProductSpecificationOption(ImportEntity importEntity)    
        {
            var productFromBase = _productService.GetProductBySku(importEntity.ProductSku);
            var allSpecificationOptions =
                _specificationAttributeService.GetSpecificationAttributes();
            var specificationMap = allSpecificationOptions.ToDictionary(i => i.Name, i => i);

            //если айди аттрибута из импорта нету в списке из связей в базе, то добавляем

            foreach (var productSpecificationAttribute in importEntity.ProductSpecifications)
            {
                if (!String.IsNullOrWhiteSpace(productSpecificationAttribute.Value))
                {
                    var importProductSpecification = _configImportCategoryEntity.ProductSpecifications.FirstOrDefault(
                        x => x.RusName.Equals(productSpecificationAttribute.Name));
                    var allowFilter =
                        importProductSpecification != null && importProductSpecification.FilterBy;
                    //пропишем айди в для specificationOptions из базы
                    GetIdForSpecificationOption(specificationMap, productSpecificationAttribute);

                    var productSpecifications =
                        _productService.GetProductBySku(importEntity.ProductSku).ProductSpecificationAttributes;
                    
                    
                    var productSpecificationAttributeOption =
                        productSpecifications.FirstOrDefault(
                            x => x.SpecificationAttributeOption.SpecificationAttributeId.Equals(productSpecificationAttribute.SpecificationId));

                    if (productSpecificationAttributeOption == null)
                    {
                        ProductSpecificationAttributeOptionMapping(productFromBase, productSpecificationAttribute, allowFilter);
                    }
                    else
                    {
                        _specificationAttributeService.DeleteProductSpecificationAttribute(
                            productSpecificationAttributeOption);

                        ProductSpecificationAttributeOptionMapping(productFromBase, productSpecificationAttribute, allowFilter);
                    }
                }
            }
        }

        private void ProductSpecificationAttributeOptionMapping(Product productFromBase,
            ProductSpecification productSpecificationAttribute, bool allowFilter)
        {
            var newProductSpecificationAttribute = new ProductSpecificationAttribute();
            newProductSpecificationAttribute.ProductId = productFromBase.Id;
            newProductSpecificationAttribute.SpecificationAttributeOptionId = productSpecificationAttribute.OptionId;
            newProductSpecificationAttribute.AllowFiltering = allowFilter;
            newProductSpecificationAttribute.ShowOnProductPage = true;
            _specificationAttributeService.InsertProductSpecificationAttribute(newProductSpecificationAttribute);
        }

        private void GetIdForSpecificationOption(Dictionary<string, SpecificationAttribute> specificationMap, ProductSpecification entity)
        {
            if (!specificationMap.ContainsKey(entity.Name))
                throw new Exception("Unknown specification name");
            var specificationAttribute = specificationMap[entity.Name];
            entity.SpecificationId = specificationAttribute.Id;
            var specificationAttributeOption = InsertSpecificationAttributeOption(specificationAttribute, entity);
            entity.OptionId = specificationAttributeOption.Id;
        }

        private SpecificationAttributeOption InsertSpecificationAttributeOption(SpecificationAttribute specificationAttribute,ProductSpecification entity)
        {
            var specAttr=specificationAttribute.SpecificationAttributeOptions.FirstOrDefault(x => x.Name.Equals(entity.Value));
            if (specAttr!=null)
            {
                return specAttr;
            }
            else
            {
                SpecificationAttributeOption specificationAttributeOption = new SpecificationAttributeOption()
                {
                    SpecificationAttributeId = specificationAttribute.Id,
                    Name = entity.Value,
                    
                };
                _specificationAttributeService.InsertSpecificationAttributeOption(specificationAttributeOption);
                return
                    specificationAttribute.SpecificationAttributeOptions.FirstOrDefault(x => x.Name.Equals(entity.Value));

            }
            
        }

        private void UpdateSpecificationOption(ImportEntity importEntity)
        {
            foreach (var entity in importEntity.ProductSpecifications)
            {
                var specificationAttribute = _specificationAttributeService.
                    GetSpecificationAttributes().
                    FirstOrDefault(x => x.Name.Equals(entity.Name));
                if (specificationAttribute != null )
                {
                    MappingAttributeSpecificationOption(entity, specificationAttribute);
                }
                else
                {
                    specificationAttribute = new SpecificationAttribute();
                    specificationAttribute.Name = entity.Name;
                    specificationAttribute.DisplayOrder = 0;
                    
                    _specificationAttributeService.InsertSpecificationAttribute(specificationAttribute);
                    MappingAttributeSpecificationOption(entity, specificationAttribute);
                   
                }
            }
        }

        private void MappingAttributeSpecificationOption(ProductSpecification entity,
            SpecificationAttribute specificationAttribute)
        {
            if (!String.IsNullOrWhiteSpace(entity.Value))
            {
                entity.SpecificationId = specificationAttribute.Id;
                var specificationOptions =
                    _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttribute(
                        entity.SpecificationId);
                //проверка связи спцификации и опции
                if (!specificationOptions.Any(x => x.Name.Equals(entity.Value)))
                {
                    var specificationAttributeOption = new SpecificationAttributeOption();
                    specificationAttributeOption.Name = entity.Value;
                    
                    specificationAttributeOption.SpecificationAttributeId = entity.SpecificationId;
                    _specificationAttributeService.InsertSpecificationAttributeOption(specificationAttributeOption);
                }
            }
        }

        private void UpdateProduct(ImportEntity importEntity)
        {
            var product = _productService.GetProductBySku(importEntity.ProductSku);
            var parentProduct = _productService.GetProductBySku(importEntity.ModelSku);
            var publish = UpdatePublish(importEntity);
            

            if (product == null)
            {

                product = CreateSimpleProduct(importEntity.ProductSku, importEntity.ProductName);
                product.Published = publish;
                product.ParentGroupedProductId = parentProduct.Id;
               
                _productService.InsertProduct(product);
                //смотрим, есть ли запись в таблице адресов
                var validateName = product.ValidateSeName(product.Name, product.Name, true);
                UpdateSlug(validateName, product);
                CreateProductCategoryMapping(product.Sku, importEntity.CategoryId);
            }
            else
            {
                product.Name = importEntity.ProductName;
                product.ParentGroupedProductId = parentProduct.Id;
                product.UpdatedOnUtc = DateTime.UtcNow;
                product.ProductType = ProductType.SimpleProduct;
                product.VisibleIndividually = true;              
                product.Published = publish;
                product.ProductTemplateId = 1;
                product.ManageInventoryMethod = ManageInventoryMethod.ManageStockByAttributes;
                product.OrderMinimumQuantity = 1;
                product.OrderMaximumQuantity = 100;//TODO: тут нужно смотреть количество из 1с
                //product.Price = storage.Price;
                product.IsShipEnabled = true;
                _productService.UpdateProduct(product);
            }
        }

        private bool UpdatePublish(ImportEntity importEntity)
        {
            bool publish;
            if (_miscOneSSetting.PublishOnlyBerikolesaStorage)
            {
                publish =
                    importEntity.Storages.Any(
                        x => String.Equals(x.Name, "Berikolesa.RU", StringComparison.CurrentCultureIgnoreCase)
                             && x.Quantity > 0);
            }
            else
            {
                publish = importEntity.Storages.Any(x => x.Price > 0 && x.Quantity > 0);
            }
            return publish;
        }

        private void UpdateSlug(string validateName, Product product)
        {
            if (_urlRecordService.GetBySlug(validateName) == null)
            {
                _urlRecordService.SaveSlug(product, validateName, 0);
            }
        }

        private void UpdateModels(ImportEntity importEntity)
        {
            //проверка сущестования модели
            var model = _productService.GetProductBySku(importEntity.ModelSku);
            var parentProduct = _productService.GetProductBySku(importEntity.BrandSku);
            if (model == null)
            {
                model = CreateGroupedProduct(importEntity.ModelSku, importEntity.ModelName);
                model.ParentGroupedProductId = parentProduct.Id;
                _productService.InsertProduct(model);
               
                var validateName = model.ValidateSeName(model.Name, model.Name, true);
                UpdateSlug(validateName, model);
               
                CreateProductCategoryMapping(model.Sku, importEntity.CategoryId);
            }
            else
            {
                model.Name = importEntity.ModelName;
                model.ParentGroupedProductId = parentProduct.Id;
                model.UpdatedOnUtc = DateTime.UtcNow;
                model.ProductType = ProductType.GroupedProduct;
                model.VisibleIndividually = true;
                model.Published = false;
                model.ProductTemplateId = 2;
                _productService.UpdateProduct(model);
            }
        }
        private void UpdateBrands(ImportEntity importEntity)
        {
            var brand = _productService.GetProductBySku(importEntity.BrandSku);
            if (brand == null)
            {
                brand = CreateGroupedProduct(importEntity.BrandSku, importEntity.BrandName);
                _productService.InsertProduct(brand);
                
                _urlRecordService.SaveSlug(brand, brand.ValidateSeName(brand.Name, brand.Name, true), 0);

                var validateName = brand.ValidateSeName(brand.Name, brand.Name, true);
                UpdateSlug(validateName, brand);

                CreateProductCategoryMapping(brand.Sku, importEntity.CategoryId);
            }
            else
            {
                //TODO:проверка на изменения данных
                brand.Name = importEntity.BrandName;
                brand.ProductType = ProductType.GroupedProduct;
                brand.VisibleIndividually = true;
                brand.ProductTemplateId = 2;
                brand.Published = false;
                brand.UpdatedOnUtc = DateTime.UtcNow;
                _productService.UpdateProduct(brand);
            }
        }
      
        void CreateProductCategoryMapping(string productSku, int categoryId)
        {

            var productCategoryMapping = new ProductCategory();
            var getModelForCategory = _productService.GetProductBySku(productSku);            
            productCategoryMapping.CategoryId = categoryId;
            productCategoryMapping.ProductId = getModelForCategory.Id;

            _categoryService.InsertProductCategory(productCategoryMapping);

        }



        private Product CreateGroupedProduct(string productSku, string productName)
        {
            var product = new Product
            {
                UpdatedOnUtc = DateTime.UtcNow,
                CreatedOnUtc = DateTime.UtcNow,
                ProductType = ProductType.GroupedProduct,
                VisibleIndividually = true,
                ProductTemplateId = 2,
                Published = false,  
                Sku = productSku,
                Name = productName
            };
            
            return product;
        }
        private Product CreateSimpleProduct(string productSku, string productName)
        {
            
            var product = new Product
            {
                UpdatedOnUtc = DateTime.UtcNow,
                CreatedOnUtc = DateTime.UtcNow,
                ProductType = ProductType.SimpleProduct,
                Published = true,
                VisibleIndividually = true,
                ProductTemplateId = 1,
                Sku = productSku,
                Name = productName,
                ManageInventoryMethod = ManageInventoryMethod.ManageStockByAttributes,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 100,//TODO: тут нужно смотреть количество из 1с
                IsShipEnabled = true
            };
           
            return product;
        }

        IEnumerable<Product> GetAllPublishedProducts(int categoryId)
        {
            var query = from p in _productRepository.Table
                        orderby p.DisplayOrder, p.Name
                        where p.Published &&
                        !p.Deleted && p.ProductCategories.Any(x => x.CategoryId.Equals(categoryId))
                        select p;
            var products = query.ToList();
            return products;
        }
        IEnumerable<Product> GetAllLastImportProducts(int categoryId)
        {
            var query = from p in _productRepository.Table
                        orderby p.DisplayOrder, p.Name
                        where p.AdminComment.Equals("last") &&
                        !p.Deleted && p.ProductCategories.Any(x => x.CategoryId.Equals(categoryId))
                        select p;
            var products = query.ToList();
            return products;
        }
    }
}