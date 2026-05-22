using Prism;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Unity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using Unity;

namespace Aksl.ActiveContentManager.ViewModels
{
    public class ActiveContentViewModel : BindableBase
    {
        #region Members
        #endregion

        #region Constructors
        public ActiveContentViewModel()
        {
            ActiveContentItems = new();
            StoreContentItems = new();
        }
        #endregion

        #region Properties
        public ObservableCollection<ActiveContentItemViewModel> ActiveContentItems { get; }
        public List<ActiveContentItemViewModel> StoreContentItems { get; }

        private ActiveContentItemViewModel _selectedContentItem;
        public ActiveContentItemViewModel SelectedContentItem
        {
            get => _selectedContentItem;
            set 
            {
                if (SetProperty<ActiveContentItemViewModel>(ref _selectedContentItem, value)) 
                {
                    SelectedIndex=GetIndexSelectedActiveContentItem();
                }
            }
        }

        private int _selectedIndex=-1;
        public int SelectedIndex
        {
            get => _selectedIndex;
            set => SetProperty<int>(ref _selectedIndex, value);
        }

        public bool CanMove
        {
            get
            {
                return ActiveContentItems is not null && ActiveContentItems.Count>1 && SelectedIndex > 0;
            }
        }
        #endregion

        #region Methods
        public void Add(ContentInformation contentInformation, bool isActive = true)
        {
            ActiveContentItemViewModel newActiveContentItemViewModel = new(contentInformation);

            if (contentInformation.ViewElement is not null)
            {
                newActiveContentItemViewModel.ViewElement = contentInformation.ViewElement;
            }

            AddCore(newActiveContentItemViewModel);
        }

        private void AddCore(ActiveContentItemViewModel newActiveContentItemViewModel, bool isActive = true)
        {
            if (!IsExistsActivContentItems(newActiveContentItemViewModel.Name, newActiveContentItemViewModel.Title))
            {
                ActiveContentItems.Add(newActiveContentItemViewModel);
            }

            // AddPropertyChanged();
            void AddPropertyChanged()
            {
                newActiveContentItemViewModel.PropertyChanged += (sender, e) =>
                {
                    if (sender is ActiveContentItemViewModel tcivm)
                    {
                        if (e.PropertyName == nameof(ActiveContentItemViewModel.IsSelected))
                        {
                            if (SelectedContentItem is null && (tcivm is not null && tcivm.IsSelected))
                            {
                                SelectedContentItem = tcivm;
                            }

                            if (SelectedContentItem is not null && (tcivm is not null && tcivm.IsSelected && tcivm != SelectedContentItem))
                            {
                                SelectedContentItem.IsSelected = false;

                                SelectedContentItem = tcivm;
                            }
                        }
                    }
                };
            }

            StoreContentItem(newActiveContentItemViewModel);

            if (isActive)
            {
                SetActiveContentItem(newActiveContentItemViewModel);
            }

            RaisePropertyChanged(nameof(CanMove));
            RaisePropertyChanged(nameof(ActiveContentItems));
        }

        private void StoreContentItem(ActiveContentItemViewModel activeContentItemViewModel)
        {
            if (!IsExistsStoreContentItems(activeContentItemViewModel.Name, activeContentItemViewModel.Title))
            {
                StoreContentItems.Add(activeContentItemViewModel);
            }
        }

        public void SetActiveContentItem(ActiveContentItemViewModel activeContentItemViewModel)
        {
            if (activeContentItemViewModel is not null && !IsEqualsContentItemViewModel(activeContentItemViewModel, SelectedContentItem))
            {
                if (SelectedContentItem is null)
                {
                    activeContentItemViewModel.IsSelected = true;
                    SelectedContentItem = activeContentItemViewModel;
                   // SelectedContentItem.IsSelected = true;
                }

                if (SelectedContentItem is not null && activeContentItemViewModel != SelectedContentItem)
                {
                    SelectedContentItem.IsSelected = false;
                    //SelectedContentItem = null;
                    // SelectedTabContentItem.ViewElementVisibility = Visibility.Collapsed;

                    activeContentItemViewModel.IsSelected = true;
                    SelectedContentItem = activeContentItemViewModel;
                  //  SelectedContentItem.IsSelected = true;
                    //  SelectedTabContentItem.ViewElementVisibility = Visibility.Visible;
                }
            }
        }

        public void SetContentItem(ContentInformation contentInformation)
        {
            var activeContentItem = GetActiveContentItemByInfo(contentInformation);
            if (activeContentItem is not null)
            {
                SetActiveContentItem(activeContentItem);
            }
            else
            {
                var storeContentItem = GetStoreContentItemViewModelByInfo(contentInformation);
                if (storeContentItem is not null)
                {
                    AddCore(storeContentItem);
                }
            }
        }

        public void RetsetContentItem(ContentInformation contentInformation)
        {
            var activeContentItem = GetActiveContentItemByInfo(contentInformation);
            if (activeContentItem is not null)
            {
                activeContentItem.ViewElement = null;
                if (contentInformation.ViewElement is not null)
                {
                    activeContentItem.ViewElement = contentInformation.ViewElement;
                }

                SetActiveContentItem(activeContentItem);
            }
            else
            {
                var storeContentItem = GetStoreContentItemViewModelByInfo(contentInformation);
                if (storeContentItem is not null)
                {
                    storeContentItem.ViewElement = null;
                    if (contentInformation.ViewElement is not null)
                    {
                        storeContentItem.ViewElement = contentInformation.ViewElement;
                    }

                    ActiveContentItems.Add(storeContentItem);

                    SetActiveContentItem(storeContentItem);
                }
            }
        }

        public void SetItemOnClose(ContentInformation contentInformation)
        {
            var activeContentItem = GetActiveContentItemByInfo(contentInformation);
            if (activeContentItem is not null)
            {
                Remove(activeContentItem);
            }
        }

        public void Remove(ActiveContentItemViewModel activeContentItemViewModel)
        {
            if (activeContentItemViewModel is not null)
            {
                if (IsExistsActivContentItems(activeContentItemViewModel.Name, activeContentItemViewModel.Title))
                {
                    if (SelectedContentItem == activeContentItemViewModel || activeContentItemViewModel.IsSelected)
                    {
                        activeContentItemViewModel.IsSelected = false;
                        SelectedContentItem = null;
                    }

                    ActiveContentItems.Remove(activeContentItemViewModel);
                }

                if (!ActiveContentItems.Any())
                {
                    SelectedContentItem = null;
                }

                RaisePropertyChanged(nameof(ActiveContentItems));
            }
        }

        public void SetItemOnSelected(ContentInformation contentInformation)
        {
            var activeTabContentItem = GetActiveContentItemByInfo(contentInformation);
            if (activeTabContentItem is not null)
            {
                SetActiveContentItem(activeTabContentItem);
            }
        }

        public void SetActiveItemByName(string name)
        {
            var activeContentItem = GetActiveContentItemByName(name);
            if (activeContentItem is not null)
            {
                SetActiveContentItem(activeContentItem);
            }
            else
            {
                var storeContentItem = GetStoreContentItemByName(name);
                if (storeContentItem is not null)
                {
                    SetActiveContentItem(activeContentItem);
                }
            }
        }

        private ActiveContentItemViewModel GetActiveContentItemByInfo(ContentInformation contentInformation)
        {
            var activeContentItemViewModel = ActiveContentItems.FirstOrDefault(aci => IsEqualsNameOrTitle(aci.Name, contentInformation.Name) || IsEqualsNameOrTitle(aci.Title, contentInformation.Title));

            return activeContentItemViewModel;
        }

        private ActiveContentItemViewModel GetActiveContentItemByName(string name)
        {
            var activeContentItemViewModel = ActiveContentItems.FirstOrDefault(aci => IsEqualsNameOrTitle(aci.Name, name));

            return activeContentItemViewModel;
        }

        public ActiveContentItemViewModel GetStoreContentItemViewModelByInfo(ContentInformation contentInformation)
        {
            var storeContentItem = StoreContentItems.FirstOrDefault(sci => IsEqualsNameOrTitle(sci.Name, contentInformation.Name) || IsEqualsNameOrTitle(sci.Title, contentInformation.Title));

            return storeContentItem;
        }

        public ActiveContentItemViewModel GetStoreContentItemByName(string name)
        {
            var storeContentItem = StoreContentItems.FirstOrDefault(stc => IsEqualsNameOrTitle(stc.ViewName, name));

            return storeContentItem;
        }

        public System.Windows.DependencyObject GetStoreViewElementByName(string name)
        {
            var storeContentItem = StoreContentItems.FirstOrDefault(sci => IsEqualsNameOrTitle(sci.Name, name));

            return storeContentItem?.ViewElement;
        }

        public System.Windows.DependencyObject GetStoreViewElementByType(Type viewType)
        {
            var storeContentItem = StoreContentItems.FirstOrDefault(stc => stc.ViewElementType == viewType);

            return storeContentItem?.ViewElement;
        }
        #endregion

        #region Move Methods
        public void ExecuteMovePrevious()
        {
            //var selectedIndex = GetIndexSelectedActiveContentItem();
            var previousActiveContentItem = ActiveContentItems[SelectedIndex - 1];
            SetActiveContentItem(previousActiveContentItem);
        }

        public bool CanMovePrevious()
        {
           // var selectedIndex = GetIndexSelectedActiveContentItem();
            return SelectedIndex > 0;
        }

        public void ExecuteMoveNext()
        {
           // var selectedIndex = GetIndexSelectedActiveContentItem();
            var nextActiveContentItem = ActiveContentItems[SelectedIndex + 1];
            SetActiveContentItem(nextActiveContentItem);
        }

        public bool CanMoveNext()
        {
           // var selectedIndex = GetIndexSelectedActiveContentItem();
            return SelectedIndex < ActiveContentItems.Count - 1;
        }

        public int GetIndexSelectedActiveContentItem()
        {
            var selectedContentInformation = SelectedContentItem.ContentInformation;
            var selectedIndex = ActiveContentItems.ToList().FindIndex(aci => IsEqualsNameOrTitle(aci.Name, selectedContentInformation.Name) || IsEqualsNameOrTitle(aci.Title, selectedContentInformation.Title));

            return selectedIndex;
        }
        #endregion

        #region Contain Methods
        private bool IsExistsActivContentItems(string name, string title)
        {
            var isAny = ActiveContentItems.Any(ti => IsEqualsNameOrTitle(ti.Name, name) || IsEqualsNameOrTitle(ti.Title, title));

            return isAny;
        }

        private bool IsExistsStoreContentItems(string name, string title)
        {
            var isAny = StoreContentItems.Any(ti => IsEqualsNameOrTitle(ti.Name, name) || IsEqualsNameOrTitle(ti.Title, title));

            return isAny;
        }

        private bool IsEqualsContentItemViewModel(ActiveContentItemViewModel activeContentItemViewModel, ActiveContentItemViewModel otherActiveContentItemViewModel)
        {
            if (activeContentItemViewModel is null || otherActiveContentItemViewModel is null)
            {
                return false;
            }

            var isEquals = (IsEqualsNameOrTitle(activeContentItemViewModel?.Name, otherActiveContentItemViewModel?.Name) ||
                            IsEqualsNameOrTitle(activeContentItemViewModel?.Title, otherActiveContentItemViewModel?.Title));

            return isEquals;
        }

        private bool IsEqualsNameOrTitle(string nameOrTitle, string otherNameOrTitle)
        {
            if (string.IsNullOrEmpty(nameOrTitle) || string.IsNullOrEmpty(otherNameOrTitle))
            {
                return false;
            }

            var isEquals = (!string.IsNullOrEmpty(nameOrTitle) && nameOrTitle.Equals(otherNameOrTitle, StringComparison.InvariantCultureIgnoreCase)) ||
                           (!string.IsNullOrEmpty(otherNameOrTitle) && otherNameOrTitle.Equals(nameOrTitle, StringComparison.InvariantCultureIgnoreCase));

            return isEquals;
        }
        #endregion
    }
}
