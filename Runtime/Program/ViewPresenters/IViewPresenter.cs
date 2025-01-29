using System;
using AdsAppView.DTO;

namespace AdsAppView.Program
{
    public interface IViewPresenter
    {
        bool Enable { get; }
        bool Background { get; }

        event Action Enabled;
        event Action Disabled;

        void Show(PopupData spriteData);
    }
}
