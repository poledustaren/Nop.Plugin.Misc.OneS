using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Misc.OneS.Models;
using Nop.Services.Catalog;

namespace Nop.Plugin.Misc.OneS.Core
{
    public  class CategoryConfigs
    {
        private readonly ICategoryService _categoryService;
        public CategoryConfigs(ICategoryService categoryService)
        {
            _categoryService = categoryService;

            Initialize();
        }
        public ConfigImportCategoryEntity OilConfig { get; set; }
        public ConfigImportCategoryEntity FastenersConfig { get; set; }

        public List<ConfigImportCategoryEntity> GetConfigList()
        {
            var configList = new List<ConfigImportCategoryEntity>
            {
                OilConfig,
                FastenersConfig
            };
            return configList;
        }
       
        void Initialize()
        {
            OilConfig = new ConfigImportCategoryEntity
            {
                Title = "Масла",
                FileName = "MaslaProducts_*.xml",
                RootName = "ProductsAvtomasla",
                RootCategoryName = "Oils",
                RootCategoryElementsName = "Oil",

                ProductSpecifications = new List<ImportProductSpecification>()
                {
                    new ImportProductSpecification() {XName = "TypeOils",RusName = "Тип масла",FilterBy = true},
                    new ImportProductSpecification() {XName = "Viscous",RusName = "Вязкость",FilterBy = true},
                    new ImportProductSpecification() {XName = "Volume",RusName = "Объем",FilterBy = true},
                    new ImportProductSpecification() {XName = "Brand", RusName = "Производитель", FilterBy = true}
                }
            };
            OilConfig.CategoryId = GetCategoryId(OilConfig.Title);

            FastenersConfig = new ConfigImportCategoryEntity
            {
                Title = "Крепеж",
                FileName = "FastenerProducts_*.xml",
                RootName = "ProductsFastener",
                RootCategoryName = "Fasteners",
                RootCategoryElementsName = "Fastener",

                ProductSpecifications = new List<ImportProductSpecification>()
                {
                    new ImportProductSpecification() {XName = "Type", RusName = "Тип крепежа", FilterBy = true},
                    new ImportProductSpecification() {XName = "Size", RusName = "Размер/шаг резьбы", FilterBy = true},
                    new ImportProductSpecification() {XName = "Skirt", RusName = "Юбка", FilterBy = true},
                    new ImportProductSpecification() {XName = "Key", RusName = "Размер ключа", FilterBy = true},
                    new ImportProductSpecification() {XName = "Length", RusName = "Длинна", FilterBy = true},
                    new ImportProductSpecification() {XName = "OpenClose", RusName = "Закрытый или открытый", FilterBy = false},
                    new ImportProductSpecification() {XName = "Secret", RusName = "Секретный", FilterBy = false},
                    new ImportProductSpecification() {XName = "Descriptor", RusName = "Комплектация", FilterBy = false},
                    new ImportProductSpecification() {XName = "Brand", RusName = "Производитель", FilterBy = true}
                }
            };
            FastenersConfig.CategoryId = GetCategoryId(FastenersConfig.Title);
        }

        public int GetCategoryId(string categoryTitle)
        {
            var category = _categoryService.GetAllCategories(categoryTitle).FirstOrDefault();
            if (category!=null)
            {
             
                return category.Id;
            }
            //добавление несуществующей категории так же для масел
            _categoryService.InsertCategory(new Category()
            {
                Name = categoryTitle,
                CategoryTemplateId = 1,
                ParentCategoryId = 0,
                PictureId = 0,
                PageSize = 20,
                AllowCustomersToSelectPageSize = false,
                PageSizeOptions = "8,4,12",
                ShowOnHomePage = false,
                IncludeInTopMenu = true,
                HasDiscountsApplied = false,
                SubjectToAcl = false,
                LimitedToStores = false,
                Published = true,
                Deleted = false,
                DisplayOrder = 0,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            });
            category = _categoryService.GetAllCategories(categoryTitle).FirstOrDefault();
            if (category!=null)
            {
                return category.Id;
            }
            return 0;
        }
    }
}