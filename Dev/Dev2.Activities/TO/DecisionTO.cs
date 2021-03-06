
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Interfaces.Infrastructure.Providers.Validation;
using Dev2.Data.Decisions.Operations;
using Dev2.Data.SystemTemplates.Models;
using Dev2.DataList;
using Dev2.DataList.Contract;
using Dev2.Interfaces;
using Dev2.Providers.Validation.Rules;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Util;
using Dev2.Utilities;
using Dev2.Validation;

namespace Dev2.TO
{
    public class DecisionTO : ValidatedObject, IDev2TOFn
    {
        public Action<DecisionTO> UpdateDisplayAction { get;  set; }
        int _indexNum;
        string _searchType;
        bool _isSearchCriteriaEnabled;
        bool _isSearchTypeFocused;
        string _matchValue;
        string _searchCriteria;
        string _from;
        string _to;
        bool _isSearchCriteriaVisible;
        bool _isFromFocused;
        bool _isToFocused;
        bool _isSinglematchCriteriaVisible;
        bool _isBetweenCriteriaVisible;
        public static readonly IList<IFindRecsetOptions> Whereoptions = FindRecsetOptions.FindAllDecision();
        Action<DecisionTO> _deleteAction;
        bool _isLast;
        public RelayCommand DeleteCommand { get;  set; }

        public DecisionTO()
            : this("Match", "Match On", "Equal", 0)
        {
        }

        public DecisionTO(string matchValue, string searchCriteria, string searchType, int indexNum, bool inserted = false, string from = "", string to = "", Action<DecisionTO> updateDisplayAction = null, Action<DecisionTO> delectAction = null)
        {
            UpdateDisplayAction = updateDisplayAction??(a=>{});
            Inserted = inserted;

            MatchValue = matchValue;
            SearchCriteria = searchCriteria;
            SearchType = searchType;
            IndexNumber = indexNum;
            IsSearchCriteriaEnabled = true;
     
            From = @from;
            To = to;
            IsSearchTypeFocused = false;
            DeleteAction = delectAction;
            DeleteCommand = new RelayCommand(a=>
            {
                if (DeleteAction != null)
                {
                    DeleteAction(this);
                }
            }, CanDelete);
            
        }

        public Action<DecisionTO> DeleteAction
        {
            get
            {
                return _deleteAction;
            }
            set
            {
                _deleteAction = value;
            }
        }

        public DecisionTO(Dev2Decision a, int ind, Action<DecisionTO> updateDisplayAction = null,Action<DecisionTO> deleteAction = null)
        {
            UpdateDisplayAction = updateDisplayAction ?? (x => { });
            Inserted = false;
            MatchValue = a.Col1;
            SearchCriteria = a.Col2;
            SearchType = DecisionDisplayHelper.GetDisplayValue(a.EvaluationFn);
            IndexNumber = ind;
            IsSearchCriteriaEnabled = true;
            IsSearchCriteriaVisible = true;
            From = a.Col2;
            To = a.Col3;
            IsSearchTypeFocused = false;
            DeleteAction = deleteAction;
            IsLast = false;
            DeleteCommand = new RelayCommand(x =>
            {
                if (DeleteAction != null)
                {
                    DeleteAction(this);
                }
            },CanDelete);

        }

       public bool CanDelete(object obj)
        {
            return !IsLast;
        }

        public bool IsLast
        {
            get
            {
                return _isLast;
            }
            set
            {
                _isLast = value;

                if(DeleteCommand != null)
                {
                    DeleteCommand.RaiseCanExecuteChanged();
                }
            }
        }

        [FindMissing]
        public string From
        {
            get
            {
                return _from;
            }
            set
            {
                _from = value;
                OnPropertyChanged();
                RaiseCanAddRemoveChanged();
                UpdateDisplay();
            }
        }

        public bool IsFromFocused { get { return _isFromFocused; } set { OnPropertyChanged(ref _isFromFocused, value); } }

        [FindMissing]
        public string To
        {
            get
            {
                return _to;
            }
            set
            {
                _to = value;
                OnPropertyChanged();
                RaiseCanAddRemoveChanged();
                UpdateDisplay();
            }
        }
        
        public bool IsToFocused { get { return _isToFocused; } set { OnPropertyChanged(ref _isToFocused, value); } }
        
        [FindMissing]
        public string SearchCriteria
        {
            get
            {
                return _searchCriteria;
            }
            set
            {
                _searchCriteria = value;
                OnPropertyChanged();
                RaiseCanAddRemoveChanged();
                UpdateDisplay();
            }
        }

        void UpdateDisplay()
        {
            if(IndexNumber == 1)
            {
                UpdateDisplayAction(this);
            }
        }

        [FindMissing]
        public string MatchValue
        {
            get
            {
                return _matchValue;
            }
            set
            {
                _matchValue = value;
               
                OnPropertyChanged();
                RaiseCanAddRemoveChanged();
                UpdateDisplay();
            }
        }

        

        //    public bool IsMatchValueFocused { get { return _isMatchValueFocused; } set { OnPropertyChanged(ref _isMatchValueFocused, value); } }

     //   public bool IsSearchCriteriaFocused { get { return _isSearchCriteriaFocused; } set { OnPropertyChanged(ref _isSearchCriteriaFocused, value); } }

        public string SearchType
        {
            get
            {
                return _searchType;
            }
            set
            {
                if (value != null)
                {
                    _searchType = FindRecordsDisplayUtil.ConvertForDisplay(value);
                    OnPropertyChanged();
                    RaiseCanAddRemoveChanged();
                    if (!string.IsNullOrEmpty(_searchType))
                    {
                        IsSearchCriteriaEnabled = true;
                    }
                    UpdateMatchVisibility(this, _searchType, Whereoptions);
                    UpdateDisplay();
                }
            }
        }

        public bool IsSearchTypeFocused { get { return _isSearchTypeFocused; } set { OnPropertyChanged(ref _isSearchTypeFocused, value); } }

        void RaiseCanAddRemoveChanged()
        {
            // ReSharper disable ExplicitCallerInfoArgument
            OnPropertyChanged("CanRemove");
            OnPropertyChanged("CanAdd");
            // ReSharper restore ExplicitCallerInfoArgument
        }

        public bool IsSearchCriteriaEnabled
        {
            get
            {
                return _isSearchCriteriaEnabled;
            }
            set
            {
                _isSearchCriteriaEnabled = value;
                OnPropertyChanged();
            }
        }

        public bool IsSearchCriteriaVisible
        {
            get
            {
                return _isSearchCriteriaVisible;
            }
            set
            {
                _isSearchCriteriaVisible = value;
                OnPropertyChanged(ref _isSearchCriteriaVisible, value);
            }
        }

        public int IndexNumber
        {
            get
            {
                return _indexNum;
            }
            set
            {
                _indexNum = value;
                OnPropertyChanged();
            }
        }

        public bool CanRemove()
        {
            if (string.IsNullOrEmpty(SearchCriteria) && string.IsNullOrEmpty(SearchType))
            {
                return true;
            }
            return false;
        }

        public bool CanAdd()
        {
            return !string.IsNullOrEmpty(SearchType);
        }

        public void ClearRow()
        {
            SearchCriteria = string.Empty;
            SearchType = "";
        }

        public bool Inserted { get; set; }

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(SearchType) && string.IsNullOrEmpty(SearchCriteria);
        }

        public override IRuleSet GetRuleSet(string propertyName, string datalist)
        {
            RuleSet ruleSet = new RuleSet();
            if (IsEmpty())
            {
                return ruleSet;
            }
            switch (propertyName)
            {
                case "SearchType":
                    if (SearchType == "Starts With" || SearchType == "Ends With" || SearchType == "Doesn't Start With" || SearchType == "Doesn't End With")
                    {
                        ruleSet.Add(new IsStringEmptyRule(() => SearchType));
                        ruleSet.Add(new IsValidExpressionRule(() => SearchType, datalist, "1"));
                    }
                    break;
                case "From":
                    if (SearchType == "Is Between" || SearchType == "Is Not Between")
                    {
                        ruleSet.Add(new IsStringEmptyRule(() => From));
                        ruleSet.Add(new IsValidExpressionRule(() => From, datalist, "1"));
                    }
                    break;
                case "To":
                    if (SearchType == "Is Between" || SearchType == "Is Not Between")
                    {
                        ruleSet.Add(new IsStringEmptyRule(() => To));
                        ruleSet.Add(new IsValidExpressionRule(() => To, datalist, "1"));
                    }
                    break;
                case "SearchCriteria":
                    if (SearchCriteria.Length == 0)
                        ruleSet.Add(new IsStringEmptyRule(() => SearchCriteria));
                    ruleSet.Add(new IsValidExpressionRule(() => SearchCriteria, datalist, "1"));
                    break;
            }

            return ruleSet;
        }

        public bool IsSinglematchCriteriaVisible
        {
            get
            {
                return _isSinglematchCriteriaVisible;
            }
            set
            {
                _isSinglematchCriteriaVisible = value;
                OnPropertyChanged();
            }
        }
        public bool IsBetweenCriteriaVisible
        {
            get
            {
                return _isBetweenCriteriaVisible;
            }
            set
            {
                _isBetweenCriteriaVisible = value;
                OnPropertyChanged();
            }
        }


        public static void UpdateMatchVisibility(DecisionTO to, string value, IList<IFindRecsetOptions> whereOptions)
        {

            var opt = whereOptions.FirstOrDefault(a => value.ToLower().StartsWith(a.HandlesType().ToLower()));
            if (opt != null)
            {
                switch (opt.ArgumentCount)
                {
                    case 1:
                        to.IsSearchCriteriaVisible = false;
                        to.IsBetweenCriteriaVisible = false;
                        to.IsSinglematchCriteriaVisible = false;
                        break;
                    case 2:
                        to.IsSearchCriteriaVisible = true;
                        to.IsBetweenCriteriaVisible = false;
                        to.IsSinglematchCriteriaVisible = true;
                        break;
                    case 3:
                        to.IsSearchCriteriaVisible = true;
                        to.IsBetweenCriteriaVisible = true;
                        to.IsSinglematchCriteriaVisible = false;
                        break;

                }
            }
        }
    }


}