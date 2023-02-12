using Blish_HUD.Content;
using Kenedia.Modules.BuildsManager.Services;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Views;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kenedia.Modules.BuildsManager.Views
{
    public class MainWindow : StandardWindow
    {
        private readonly Data _data;
        private readonly FlowPanel _contentPanel;

        public MainWindow(AsyncTexture2D background, Rectangle windowRegion, Rectangle contentRegion, Data data) : base(background, windowRegion, contentRegion)
        {
            _data = data;

            _contentPanel = new FlowPanel()
            {
                Parent = this,
                WidthSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                HeightSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                CanScroll= true,
            };

            foreach (var item in _data.Weapons)
            {
                _ = new Image()
                {
                    Parent = _contentPanel,
                    Texture = item.Value.Icon,
                    SetLocalizedTooltip = () => item.Value.Name,
                    Size = new(32),
                };
            }

            foreach (var item in _data.Armors)
            {
                _ = new Image()
                {
                    Parent = _contentPanel,
                    Texture = item.Value.Icon,
                    SetLocalizedTooltip = () => item.Value.Name,
                    Size = new(32),
                };
            }

            foreach (var item in _data.Trinkets)
            {
                _ = new Image()
                {
                    Parent = _contentPanel,
                    Texture = item.Value.Icon,
                    SetLocalizedTooltip = () => item.Value.Name,
                    Size = new(32),
                };
            }

            foreach (var item in _data.Upgrades)
            {
                _ = new Image()
                {
                    Parent = _contentPanel,
                    Texture = item.Value.Icon,
                    SetLocalizedTooltip = () => item.Value.Name,
                    Size = new(32),
                };
            }

            foreach (var profession in _data.Professions)
            {
                _ = new Image()
                {
                    Parent = _contentPanel,
                    Texture = profession.Value.Icon,
                    SetLocalizedTooltip = () => profession.Value.Name,
                    Size = new(32),
                };

                _ = new Image()
                {
                    Parent = _contentPanel,
                    Texture = profession.Value.IconBig,
                    SetLocalizedTooltip = () => profession.Value.Name,
                    Size = new(32),
                };

                foreach (var e in profession.Value.Specializations)
                {
                    _ = new Image()
                    {
                        Parent = _contentPanel,
                        Texture = e.Value.Icon,
                        SetLocalizedTooltip = () => e.Value.Name,
                        Size = new(32),
                    };

                    _ = new Image()
                    {
                        Parent = _contentPanel,
                        Texture = e.Value.Background,
                        SetLocalizedTooltip = () => e.Value.Name,
                        Size = new(32),
                    };

                    foreach (var t in e.Value.MajorTraits)
                    {
                        _ = new Image()
                        {
                            Parent = _contentPanel,
                            Texture = t.Value.Icon,
                            SetLocalizedTooltip = () => t.Value.Name,
                            Size = new(32),
                        };
                    }

                    foreach (var t in e.Value.MinorTraits)
                    {
                        _ = new Image()
                        {
                            Parent = _contentPanel,
                            Texture = t.Value.Icon,
                            SetLocalizedTooltip = () => t.Value.Name,
                            Size = new(32),
                        };
                    }
                }

                foreach (var t in profession.Value.Skills)
                {
                    _ = new Image()
                    {
                        Parent = _contentPanel,
                        Texture = t.Value.Icon,
                        SetLocalizedTooltip = () => t.Value.Name,
                        Size = new(32),
                    };
                }
            }

            foreach (var pet in _data.Pets)
            {
                _ = new Image()
                {
                    Parent = _contentPanel,
                    Texture = pet.Value.Icon,
                    SetLocalizedTooltip = () => pet.Value.Name,
                    Size = new(32),
                };

                foreach (var skill in pet.Value.Skills)
                {
                    _ = new Image()
                    {
                        Parent = _contentPanel,
                        Texture = skill.Value.Icon,
                        SetLocalizedTooltip = () => skill.Value.Name,
                        Size = new(32),
                    };
                }
            }
        }
    }
}
