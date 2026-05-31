using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

using Prism;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Unity;
using Unity;

using Aksl.Dialogs.Services;
using Aksl.Infrastructure;
using Aksl.Toolkit.Controls;

namespace Aksl.Modules.HamburgerMenuNavigationSideBar.ViewModels
{
    public class MenuItemViewModel : NodeViewModel
    {
        #region Members
        protected readonly IEventAggregator _eventAggregator;
        private readonly MenuItem _menuItem;
        #endregion

        #region Constructors
        public MenuItemViewModel(int groupIndex, int index, MenuItem menuItem)
        {
            _eventAggregator = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IEventAggregator>();
            GroupIndex = groupIndex;
            Index = index;
            _menuItem = menuItem;
        }

        public MenuItemViewModel() : base()
        {
            _eventAggregator = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IEventAggregator>();

            _menuItem = null;
            //Parent = null;

            //_children = new();
        }

        public MenuItemViewModel(MenuItem menuItem, MenuItemViewModel parent) : base(menuItem.Name, menuItem.Title, parent)
        {
            _eventAggregator = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IEventAggregator>();

            _menuItem = menuItem;

            //Parent = parent;
            //Parent?.Children.Add(this);

            //_children = new();
        }
        #endregion

        #region Properties
        public MenuItem MenuItem => _menuItem;
        public int GroupIndex { get; set; }
        public int Index { get; set; }
        public string WorkspaceViewEventName { get; set; }
        private bool IsNextNavigation => _menuItem.IsNextNavigation;
        private bool HasNavigationName => !string.IsNullOrEmpty(_menuItem.NavigationName);
        private bool IsNexOnNotLeaf => _menuItem.IsNexOnNotLeaf;

        private bool _isSelected = false;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (SetProperty<bool>(ref _isSelected, value))
                {
                    if (IsLeaf && _isSelected)
                    {
                        var dialogViewService = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IDialogViewService>();

                        try
                        {
                            var contentInformation = ActiveContentHelper.CreateContentInformationAsync(_menuItem);

                            // ActiveContentHelper.AddViewToContentAsync(_menuItem, ActiveContentNames.RightContentHamburgerMenuNavigationSideBar, dialogViewService).Await();
                        }
                        catch (Exception ex)
                        {
                            dialogViewService.AlertAsync(message: $"Unable to find \"{ex.Message}\".", title: $"Error:Add View");
                        }
                    }
                }
            }
        }

        public PackIconKind IconKind
        {
            get
            {
                PackIconKind kind = PackIconKind.None;

                _ = Enum.TryParse(_menuItem.IconKind, out kind);

                return kind;
            }
        }

        private bool _isPaneOpen = false;
        public bool IsPaneOpen
        {
            get => _isPaneOpen;
            set => SetProperty<bool>(ref _isPaneOpen, value);
        }

        protected bool _isEnabled = true;
        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty<bool>(ref _isEnabled, value);
        }
        #endregion
    }
}
