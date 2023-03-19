using Blish_HUD;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kenedia.Modules.BuildsManager.Controls.GearPage
{
    public class InfusionControl : Blish_HUD.Controls.Container
    {
        private Template _template;
        private readonly bool _created;

        private readonly DetailedTexture _infusion = new() { TextureRegion = new(36, 36, 56, 56) };
        private readonly DetailedTexture _identifier = new(517199) { TextureRegion = new(8, 8, 48, 48) };

        private readonly (DetailedTexture, NumberBox) _power = new(new(66722) { TextureRegion = new(4, 4, 24, 24) }, null);
        private readonly (DetailedTexture, NumberBox) _thoughness = new(new(156612) { TextureRegion = new(4, 4, 24, 24) }, null);
        private readonly (DetailedTexture, NumberBox) _vitality = new(new(156613) { TextureRegion = new(4, 4, 24, 24) }, null);
        private readonly (DetailedTexture, NumberBox) _precision = new(new(156609) { TextureRegion = new(4, 4, 24, 24) }, null);
        private readonly (DetailedTexture, NumberBox) _ferocity = new(new(156602) { TextureRegion = new(4, 4, 24, 24) }, null);
        private readonly (DetailedTexture, NumberBox) _condition = new(new(156600) { TextureRegion = new(4, 4, 24, 24) }, null);
        private readonly (DetailedTexture, NumberBox) _expertise = new(new(156601) { TextureRegion = new(4, 4, 24, 24) }, null);
        private readonly (DetailedTexture, NumberBox) _concentration = new(new(156599) { TextureRegion = new(4, 4, 24, 24) }, null);

        private Rectangle _headerBounds;

        public InfusionControl()
        {
            _infusion.Texture = BuildsManager.ModuleInstance.ContentsManager.GetTexture(@"textures\infusionslot.png");

            _power.Item2 = new() { Parent = this, MinValue = 0, MaxValue = 16, Value = 0, };
            _thoughness.Item2 = new() { Parent = this, MinValue = 0, MaxValue = 16, Value = 0, };
            _vitality.Item2 = new() { Parent = this, MinValue = 0, MaxValue = 16, Value = 0, };
            _precision.Item2 = new() { Parent = this, MinValue = 0, MaxValue = 16, Value = 0, };
            _ferocity.Item2 = new() { Parent = this, MinValue = 0, MaxValue = 16, Value = 0, };
            _condition.Item2 = new() { Parent = this, MinValue = 0, MaxValue = 16, Value = 0, };
            _expertise.Item2 = new() { Parent = this, MinValue = 0, MaxValue = 16, Value = 0, };
            _concentration.Item2 = new() { Parent = this, MinValue = 0, MaxValue = 16, Value = 0, };

            _created = true;
            Size = new(380, 300);
        }

        public Template Template
        {
            get => _template; set
            {
                var temp = _template;
                if (Common.SetProperty(ref _template, value, ApplyTemplate, value != null))
                {
                    if (temp != null) temp.Changed -= TemplateChanged;
                    if (temp != null) temp.Changed -= TemplateChanged;

                    if (_template != null) _template.Changed += TemplateChanged;
                    if (_template != null) _template.Changed += TemplateChanged;
                }
            }
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

            _identifier.Draw(this, spriteBatch, RelativeMousePosition);
            //_infusion.Draw(this, spriteBatch, RelativeMousePosition);
            spriteBatch.DrawStringOnCtrl(this, "Infusions", Content.DefaultFont18, _headerBounds, Color.White);

            _power.Item1.Draw(this, spriteBatch);
            _precision.Item1.Draw(this, spriteBatch);
            _ferocity.Item1.Draw(this, spriteBatch);
            _thoughness.Item1.Draw(this, spriteBatch);

            _vitality.Item1.Draw(this, spriteBatch);
            _condition.Item1.Draw(this, spriteBatch);
            _expertise.Item1.Draw(this, spriteBatch);
            _concentration.Item1.Draw(this, spriteBatch);
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            if (_created)
            {
                int size = 64;
                int height = _power.Item2.Height;

                _identifier.Bounds = new(0, 0, size / 2, size / 2);
                //_infusion.Bounds = new(_identifier.Bounds.Right + 2, 0, size, size);
                _headerBounds = new(_identifier.Bounds.Right + 5, 0, Width - (_identifier.Bounds.Right + 5), size / 2);

                _power.Item1.Bounds = new(0, _headerBounds.Bottom + 5, height, height);
                _power.Item2.Location = new(_power.Item1.Bounds.Right + 4, _power.Item1.Bounds.Top);

                _precision.Item1.Bounds = new(0, _power.Item1.Bounds.Bottom + 5, height, height);
                _precision.Item2.Location = new(_precision.Item1.Bounds.Right + 4, _precision.Item1.Bounds.Top);

                _ferocity.Item1.Bounds = new(0, _precision.Item1.Bounds.Bottom + 5, height, height);
                _ferocity.Item2.Location = new(_ferocity.Item1.Bounds.Right + 4, _ferocity.Item1.Bounds.Top);

                _thoughness.Item1.Bounds = new(0, _ferocity.Item1.Bounds.Bottom + 5, height, height);
                _thoughness.Item2.Location = new(_thoughness.Item1.Bounds.Right + 4, _thoughness.Item1.Bounds.Top);

                _condition.Item1.Bounds = new(_power.Item2.Right + 25, _power.Item1.Bounds.Top, height, height);
                _condition.Item2.Location = new(_condition.Item1.Bounds.Right + 4, _condition.Item1.Bounds.Top);

                _expertise.Item1.Bounds = new(_condition.Item1.Bounds.Left, _condition.Item1.Bounds.Bottom + 5, height, height);
                _expertise.Item2.Location = new(_expertise.Item1.Bounds.Right + 4, _expertise.Item1.Bounds.Top);

                _concentration.Item1.Bounds = new(_expertise.Item1.Bounds.Left, _expertise.Item1.Bounds.Bottom + 5, height, height);
                _concentration.Item2.Location = new(_concentration.Item1.Bounds.Right + 4, _concentration.Item1.Bounds.Top);

                _vitality.Item1.Bounds = new(_concentration.Item1.Bounds.Left, _concentration.Item1.Bounds.Bottom + 5, height, height);
                _vitality.Item2.Location = new(_vitality.Item1.Bounds.Right + 4, _vitality.Item1.Bounds.Top);
            }
        }

        public void ApplyTemplate()
        {
            //_power.Item2.Value = Template.GearTemplate.Infusions[AttributeType.Power];
            //_precision.Item2.Value = Template.GearTemplate.Infusions[AttributeType.Precision];
            //_ferocity.Item2.Value = Template.GearTemplate.Infusions[AttributeType.CritDamage];
            //_thoughness.Item2.Value = Template.GearTemplate.Infusions[AttributeType.Toughness];
            //_condition.Item2.Value = Template.GearTemplate.Infusions[AttributeType.ConditionDamage];
            //_expertise.Item2.Value = Template.GearTemplate.Infusions[AttributeType.ConditionDuration];
            //_concentration.Item2.Value = Template.GearTemplate.Infusions[AttributeType.BoonDuration];
            //_vitality.Item2.Value = Template.GearTemplate.Infusions[AttributeType.Vitality];
        }

        private void TemplateChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ApplyTemplate();
        }
    }
}
