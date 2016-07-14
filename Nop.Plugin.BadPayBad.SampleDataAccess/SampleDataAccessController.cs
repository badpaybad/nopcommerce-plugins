using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Nop.Core.Data;
using Nop.Services.Localization;
using Nop.Services.Seo;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Localization;

namespace Nop.Plugin.BadPayBad.SampleDataAccess
{
    public class SampleFrontEndModel
    {
        public List<SampleTableInDb> Items { get; set; }
    }

    public class SampleBackEndModel : ILocalizedModel<SampleBackEndLocalizeModel>
    {
        public SampleBackEndModel()
        {
            Locales = new List<SampleBackEndLocalizeModel>();
        }
        public IList<SampleBackEndLocalizeModel> Locales { get; set; }
  
        public int Id { get; set; }

        [AllowHtml]
        public string Name { get; set; }

        [AllowHtml]
        public string Version { get; set; }
  }

    public class SampleBackEndLocalizeModel : ILocalizedModelLocal
    {
        [AllowHtml]
        public string Name { get; set; }

        [AllowHtml]
        public string Version { get; set; }

        public int LanguageId { get; set; }
    }

    public class SampleDataAccessController : BasePluginController
    {
        private IRepository<SampleTableInDb> _repository;
        private SampleTableInDbContext _context;
        private readonly ILanguageService _languageService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IUrlRecordService _urlRecordService;

        public SampleDataAccessController(IRepository<SampleTableInDb> repository,
            SampleTableInDbContext context, ILanguageService languageService
            , ILocalizedEntityService localizedEntityService, IUrlRecordService urlRecordService)
        {
            _repository = repository;
            _context = context;
            _languageService = languageService;
            _localizedEntityService = localizedEntityService;
            _urlRecordService = urlRecordService;
        }


        public ActionResult FrontEnd()
        {
            var model = new SampleFrontEndModel();

            InitSampleData();

            //model.Items = _context.SqlQuery<SampleTableInDb>("select * from SampleTableInDb ").ToList();
            model.Items = _repository.Table.ToList();

            return PartialView("~/Plugins/BadPayBad.SampleDataAccess/FrontEnd.cshtml", model);
        }

        public ActionResult BackEnd()
        {
            var model = new SampleBackEndModel();

            var sampleObj = InitSampleData();

            model.Id = sampleObj.Id;
            model.Name = sampleObj.Name;
            model.Version = sampleObj.Version;

            //model.Items = _context.SqlQuery<SampleTableInDb>("select * from SampleTableInDb ").ToList();

            //Store_SampleTableInDb_SelectAll you need create store procedure in db with name "Store_SampleTableInDb_SelectAll"
            //model.Items = _context.ExecuteStoredProcedureList<SampleTableInDb>("Store_SampleTableInDb_SelectAll").ToList();

            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = sampleObj.GetLocalized(x => x.Name, languageId, false, false);
                locale.Version = sampleObj.GetLocalized(x => x.Version, languageId, false, false);
            });
            
            return PartialView("~/Plugins/BadPayBad.SampleDataAccess/BackEnd.cshtml", model);
        }


        [HttpPost]
        public ActionResult BackEnd(SampleBackEndModel modelPosted)
        {
            SampleTableInDb dbObj = _repository.GetById(modelPosted.Id);
            
            dbObj.Name = modelPosted.Name;
            dbObj.Version = modelPosted.Version;

            _repository.Update(dbObj);
          
            //locales
            UpdateLocales(dbObj, modelPosted);

            return PartialView("~/Plugins/BadPayBad.SampleDataAccess/BackEnd.cshtml", modelPosted);
        }


        protected virtual void UpdateLocales(SampleTableInDb obj, SampleBackEndModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(obj,
                    x => x.Name,
                    localized.Name,
                    localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(obj,
                    x => x.Version,
                    localized.Version,
                    localized.LanguageId);
            }
        }

        /// <summary>
        /// for this sample only, you may want to remove this
        /// </summary>
        /// <returns></returns>
        private SampleTableInDb InitSampleData()
        {
            var xxx = _repository.Table.FirstOrDefault();
            if (xxx != null) return xxx;

            var sampleTableInDb = new SampleTableInDb()
            {
                Name = "Sample version 1.0.0.0",
                Version = "1.0.0.0"
            };
            _repository.Insert(sampleTableInDb);

            return sampleTableInDb;
        }
    }
}