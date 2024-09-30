using Blish_HUD.Graphics.UI;
using Kenedia.Modules.BuildsManager.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kenedia.Modules.BuildsManager.Views
{
    public class TagEditView : View
    {
        public TagEditView(TemplateTags templateTags, TagGroups tagGroups)
        {
            TemplateTags = templateTags;
            TagGroups = tagGroups;
        }

        public TemplateTags TemplateTags { get; }

        public TagGroups TagGroups { get; }

        protected override void Build(Blish_HUD.Controls.Container buildPanel)
        {
            base.Build(buildPanel);
        }
    }
}
