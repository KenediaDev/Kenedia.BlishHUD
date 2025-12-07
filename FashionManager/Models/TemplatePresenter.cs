using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.FashionManager.Services;

namespace Kenedia.Modules.FashionManager.Models
{
    public class TemplatePresenter
    {
        public TemplatePresenter(FashionTemplateFactory fashionTemplateFactory)
        {
            FashionTemplateFactory = fashionTemplateFactory;
        }

        public FashionTemplate Template
        {
            get; set
            {
                value ??= FashionTemplateFactory.CreateFashionTemplate();

                if (Common.SetProperty(ref field, value))
                {
                    SetupTemplate(this, new ValueChangedEventArgs<FashionTemplate>(null, value));
                }
            }
        }

        public FashionTemplateFactory FashionTemplateFactory { get; }

        private void SetupTemplate(object sender, ValueChangedEventArgs<FashionTemplate> e)
        {
            if (e.OldValue != null)
            {

            }

            if (e.NewValue != null)
            {

            }
        }
    }
}
