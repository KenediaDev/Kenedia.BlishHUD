﻿using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kenedia.Modules.Core.Views
{
    public class BaseView : View
    {
        protected Container Root { get; private set; }

        protected override void Build(Container buildPanel)
        {
            base.Build(buildPanel);
            Root = buildPanel;
        }

        protected override void Unload()
        {
            base.Unload();
        }
    }
}
