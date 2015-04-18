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
                Title = "�����",
                FileName = "MaslaProducts_*.xml",
                RootName = "ProductsAvtomasla",
                RootCategoryName = "Oils",
                RootCategoryElementsName = "Oil",

                ProductSpecifications = new List<ImportProductSpecification>()
                {
                    new ImportProductSpecification() {XName = "TypeOils",RusName = "��� �����",FilterBy = true},
                    new ImportProductSpecification() {XName = "Viscous",RusName = "��������",FilterBy = true},
                    new ImportProductSpecification() {XName = "Volume",RusName = "�����",FilterBy = true},
                    new ImportProductSpecification() {XName = "Brand", RusName = "�������������", FilterBy = true}
                }
            };
            OilConfig.CategoryId = GetCategoryId(OilConfig.Title);

            FastenersConfig = new ConfigImportCategoryEntity
            {
                Title = "������",
                FileName = "FastenerProducts_*.xml",
                RootName = "ProductsFastener",
                RootCategoryName = "Fasteners",
                RootCategoryElementsName = "Fastener",

                ProductSpecifications = new List<ImportProductSpecification>()
                {
                    new ImportProductSpecification() {XName = "Type", RusName = "��� �������", FilterBy = true},
                    new ImportProductSpecification() {XName = "Size", RusName = "������/��� ������", FilterBy = true},
                    new ImportProductSpecification() {XName = "Skirt", RusName = "����", FilterBy = true},
                    new ImportProductSpecification() {XName = "Key", RusName = "������ �����", FilterBy = true},
                    new ImportProductSpecification() {XName = "Length", RusName = "������", FilterBy = true},
                    new ImportProductSpecification() {XName = "OpenClose", RusName = "�������� ��� ��������", FilterBy = false},
                    new ImportProductSpecification() {XName = "Secret", RusName = "���������", FilterBy = false},
                    new ImportProductSpecification() {XName = "Descriptor", RusName = "������������", FilterBy = false},
                    new ImportProductSpecification() {XName = "Brand", RusName = "�������������", FilterBy = true}
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
            //���������� �������������� ��������� ��� �� ��� �����
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