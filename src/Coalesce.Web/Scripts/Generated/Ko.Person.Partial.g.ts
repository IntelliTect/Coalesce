
/// <reference path="../coalesce.dependencies.d.ts" />

// Knockout View Model for: Person
// Generated by IntelliTect.Coalesce

module ViewModels {

	export class PersonPartial extends Coalesce.BaseViewModel
    {
        protected modelName = "Person";
        protected primaryKeyName: keyof this = "personId";
        protected modelDisplayName = "Person";

        protected apiController = "/Person";
        protected viewController = "/Person";

        /** Behavioral configuration for all instances of Person. Can be overidden on each instance via instance.coalesceConfig. */
        public static coalesceConfig: Coalesce.ViewModelConfiguration<Person>
            = new Coalesce.ViewModelConfiguration<Person>(Coalesce.GlobalConfiguration.viewModel);

        /** Behavioral configuration for the current Person instance. */
        public coalesceConfig: Coalesce.ViewModelConfiguration<this>
            = new Coalesce.ViewModelConfiguration<PersonPartial>(Person.coalesceConfig);
    
        /** 
            The namespace containing all possible values of this.dataSource.
        */
        public dataSources: typeof ListViewModels.PersonDataSources = ListViewModels.PersonDataSources;
    

        /** ID for the person object. */
        public personId: KnockoutObservable<number> = ko.observable(null);
        /** Title of the person, Mr. Mrs, etc. */
        public title: KnockoutObservable<number> = ko.observable(null);
        /** Text value for enumeration Title */
        public titleText: KnockoutComputed<string> = ko.pureComputed(() => {
            for(var i = 0; i < this.titleValues.length; i++){
                if (this.titleValues[i].id == this.title()){
                    return this.titleValues[i].value;
                }
            }
        });
        /** First name of the person. */
        public firstName: KnockoutObservable<string> = ko.observable(null);
        /** Last name of the person */
        public lastName: KnockoutObservable<string> = ko.observable(null);
        /** Email address of the person */
        public email: KnockoutObservable<string> = ko.observable(null);
        /** Genetic Gender of the person. */
        public gender: KnockoutObservable<number> = ko.observable(null);
        /** Text value for enumeration Gender */
        public genderText: KnockoutComputed<string> = ko.pureComputed(() => {
            for(var i = 0; i < this.genderValues.length; i++){
                if (this.genderValues[i].id == this.gender()){
                    return this.genderValues[i].value;
                }
            }
        });
        /** List of cases assigned to the person */
        public casesAssigned: KnockoutObservableArray<ViewModels.Case> = ko.observableArray([]);
        /** List of cases reported by the person. */
        public casesReported: KnockoutObservableArray<ViewModels.Case> = ko.observableArray([]);
        public birthDate: KnockoutObservable<moment.Moment> = ko.observable(null);
        public lastBath: KnockoutObservable<moment.Moment> = ko.observable(null);
        public nextUpgrade: KnockoutObservable<moment.Moment> = ko.observable(null);
        public personStats: KnockoutObservable<ViewModels.PersonStats> = ko.observable(null);
        /** Calculated name of the person. eg., Mr. Michael Stokesbary. */
        public name: KnockoutObservable<string> = ko.observable(null);
        /** Company ID this person is employed by */
        public companyId: KnockoutObservable<number> = ko.observable(null);
        /** Company loaded from the Company ID */
        public company: KnockoutObservable<ViewModels.Company> = ko.observable(null);

       
        /** Display text for PersonStats */
        public personStatsText: KnockoutComputed<string>;
        /** Display text for Company */
        public companyText: KnockoutComputed<string>;
        

        /** Add object to casesAssigned */
        public addToCasesAssigned = (autoSave?: boolean | null): Case => {
            var newItem = new Case();
            if (typeof(autoSave) == 'boolean'){
                newItem.coalesceConfig.autoSaveEnabled(autoSave);
            }
            newItem.parent = this;
            newItem.parentCollection = this.casesAssigned;
            newItem.isExpanded(true);
            newItem.assignedToId(this.personId());
            this.casesAssigned.push(newItem);
            return newItem;
        };

        /** ListViewModel for CasesAssigned. Allows for loading subsets of data. */
        public casesAssignedList: (loadImmediate?: boolean) => ListViewModels.CaseList;
        
        /** Add object to casesReported */
        public addToCasesReported = (autoSave?: boolean | null): Case => {
            var newItem = new Case();
            if (typeof(autoSave) == 'boolean'){
                newItem.coalesceConfig.autoSaveEnabled(autoSave);
            }
            newItem.parent = this;
            newItem.parentCollection = this.casesReported;
            newItem.isExpanded(true);
            newItem.reportedById(this.personId());
            this.casesReported.push(newItem);
            return newItem;
        };

        /** ListViewModel for CasesReported. Allows for loading subsets of data. */
        public casesReportedList: (loadImmediate?: boolean) => ListViewModels.CaseList;
        
        /** Url for a table view of all members of collection CasesAssigned for the current object. */
        public casesAssignedListUrl: KnockoutComputed<string> = ko.computed(
            () => this.coalesceConfig.baseViewUrl() + '/Case/Table?filter.assignedToId=' + this.personId(),
            null, { deferEvaluation: true }
        );
        /** Url for a table view of all members of collection CasesReported for the current object. */
        public casesReportedListUrl: KnockoutComputed<string> = ko.computed(
            () => this.coalesceConfig.baseViewUrl() + '/Case/Table?filter.reportedById=' + this.personId(),
            null, { deferEvaluation: true }
        );

        /** Pops up a stock editor for object personStats */
        public showPersonStatsEditor: (callback?: any) => void;
        /** Pops up a stock editor for object company */
        public showCompanyEditor: (callback?: any) => void;


        /** Array of all possible names & values of enum title */
        public titleValues: EnumValue[] = [ 
            { id: 0, value: 'Mr' },
            { id: 1, value: 'Ms' },
            { id: 2, value: 'Mrs' },
            { id: 4, value: 'Miss' },
        ];
        /** Array of all possible names & values of enum gender */
        public genderValues: EnumValue[] = [ 
            { id: 0, value: 'Non Specified' },
            { id: 1, value: 'Male' },
            { id: 2, value: 'Female' },
        ];

        
        
        /**
            Invoke server method Rename.
            Sets the FirstName to the given text.
        */
        public rename = (name: string, callback: (result: ViewModels.Person) => void = null, reload: boolean = true): JQueryPromise<any> => {

            this.renameIsLoading(true);
            this.renameMessage('');
            this.renameWasSuccessful(null);
            return $.ajax({ method: "Post",
                        url: this.coalesceConfig.baseApiUrl() + this.apiController + "/Rename",
                        data: { id: this.myId, name: name },
                        xhrFields: { withCredentials: true } })
            .done((data) => {
                this.isDirty(false);
				this.renameResultRaw(data.object);
                this.renameMessage('');
                this.renameWasSuccessful(true);
                if (!this.renameResult()){
                    this.renameResult(new Person(data.object));
                } else {
                    this.renameResult().loadFromDto(data.object);
                }

                // The return type is the type of the object, load it.
                this.loadFromDto(data.object, true)
                if (typeof(callback) == "function") {
                    var result = this.renameResult();
                    callback(result);
                }
            })
            .fail((xhr) => {
                var errorMsg = "Unknown Error";
                if (xhr.responseJSON && xhr.responseJSON.message) errorMsg = xhr.responseJSON.message;
                this.renameWasSuccessful(false);
                this.renameMessage(errorMsg);
    
                if (this.coalesceConfig.showFailureAlerts())
                    this.coalesceConfig.onFailure()(this as any, "Could not call method rename: " + errorMsg);
            })
            .always(() => {
                this.renameIsLoading(false);
            });
        } 
        /** Result of server method (Rename) strongly typed in a observable. */
        public renameResult: KnockoutObservable<ViewModels.Person> = ko.observable(null);
        /** Raw result object of server method (Rename) simply wrapped in an observable. */
        public renameResultRaw: KnockoutObservable<any> = ko.observable();
        /** True while the server method (Rename) is being called */
        public renameIsLoading: KnockoutObservable<boolean> = ko.observable(false);
        /** Error message for server method (Rename) if it fails. */
        public renameMessage: KnockoutObservable<string> = ko.observable(null);
        /** True if the server method (Rename) was successful. */
        public renameWasSuccessful: KnockoutObservable<boolean> = ko.observable(null);
        /** Presents a series of input boxes to call the server method (Rename) */
        public renameUi = (callback: () => void = null, reload: boolean = true): JQueryPromise<any> => {
            var $promptVal: string = null;
            $promptVal = prompt('Name');
            if ($promptVal === null) return;
            var name: string = $promptVal;
              
            return this.rename(name, callback, reload);
        }
        /** Presents a modal with input boxes to call the server method (Rename). Depends on a modal existing with id #method-Rename. */
        public renameModal = (callback: () => void = null, reload: boolean = true): void => {
            $('#method-Rename').modal();
            $('#method-Rename').on('shown.bs.modal', () => {
                $('#method-Rename .btn-ok').unbind('click');
                $('#method-Rename .btn-ok').click(() => {
                    this.renameWithArgs(null, callback, reload);
                    $('#method-Rename').modal('hide');
                });
            });
        }
        /** Calls server method (Rename) with an instance of PersonPartial.RenameArgs, or the value of renameArgs if not specified. */
        public renameWithArgs = (args?: PersonPartial.RenameArgs, callback?: (result: ViewModels.Person) => void, reload: boolean = true) => {
            if (!args) args = this.renameArgs;
            return this.rename(args.name(), callback, reload);
        }
        /** Object that can be easily bound to fields to allow data entry for the method */
        public renameArgs = new PersonPartial.RenameArgs(); 
        
        
        
        /**
            Invoke server method ChangeSpacesToDashesInName.
            Removes spaces from the name and puts in dashes
        */
        public changeSpacesToDashesInName = (callback: (result: any) => void = null, reload: boolean = true): JQueryPromise<any> => {

            this.changeSpacesToDashesInNameIsLoading(true);
            this.changeSpacesToDashesInNameMessage('');
            this.changeSpacesToDashesInNameWasSuccessful(null);
            return $.ajax({ method: "Post",
                        url: this.coalesceConfig.baseApiUrl() + this.apiController + "/ChangeSpacesToDashesInName",
                        data: { id: this.myId },
                        xhrFields: { withCredentials: true } })
            .done((data) => {
                this.isDirty(false);
				this.changeSpacesToDashesInNameResultRaw(data.object);
                this.changeSpacesToDashesInNameMessage('');
                this.changeSpacesToDashesInNameWasSuccessful(true);
                this.changeSpacesToDashesInNameResult(data.object);

                if (typeof(callback) != "function") return;
                var result = this.changeSpacesToDashesInNameResult();
                if (reload) {
                  this.load(null, () => callback(result));
                } else {
                  callback(result);
                }
            })
            .fail((xhr) => {
                var errorMsg = "Unknown Error";
                if (xhr.responseJSON && xhr.responseJSON.message) errorMsg = xhr.responseJSON.message;
                this.changeSpacesToDashesInNameWasSuccessful(false);
                this.changeSpacesToDashesInNameMessage(errorMsg);
    
                if (this.coalesceConfig.showFailureAlerts())
                    this.coalesceConfig.onFailure()(this as any, "Could not call method changeSpacesToDashesInName: " + errorMsg);
            })
            .always(() => {
                this.changeSpacesToDashesInNameIsLoading(false);
            });
        } 
        /** Result of server method (ChangeSpacesToDashesInName) strongly typed in a observable. */
        public changeSpacesToDashesInNameResult: KnockoutObservable<any> = ko.observable(null);
        /** Raw result object of server method (ChangeSpacesToDashesInName) simply wrapped in an observable. */
        public changeSpacesToDashesInNameResultRaw: KnockoutObservable<any> = ko.observable();
        /** True while the server method (ChangeSpacesToDashesInName) is being called */
        public changeSpacesToDashesInNameIsLoading: KnockoutObservable<boolean> = ko.observable(false);
        /** Error message for server method (ChangeSpacesToDashesInName) if it fails. */
        public changeSpacesToDashesInNameMessage: KnockoutObservable<string> = ko.observable(null);
        /** True if the server method (ChangeSpacesToDashesInName) was successful. */
        public changeSpacesToDashesInNameWasSuccessful: KnockoutObservable<boolean> = ko.observable(null);
        /** Presents a series of input boxes to call the server method (ChangeSpacesToDashesInName) */
        public changeSpacesToDashesInNameUi = (callback: () => void = null, reload: boolean = true): JQueryPromise<any> => {
            var $promptVal: string = null;
            return this.changeSpacesToDashesInName(callback, reload);
        }
        /** Presents a modal with input boxes to call the server method (ChangeSpacesToDashesInName). Depends on a modal existing with id #method-ChangeSpacesToDashesInName. */
        public changeSpacesToDashesInNameModal = (callback: () => void = null, reload: boolean = true): void => {
            this.changeSpacesToDashesInNameUi(callback, reload);
        }
        
        
        
        /**
            Invoke server method FullNameAndAge.
        */
        public fullNameAndAge = (callback: (result: string) => void = null, reload: boolean = true): JQueryPromise<any> => {

            this.fullNameAndAgeIsLoading(true);
            this.fullNameAndAgeMessage('');
            this.fullNameAndAgeWasSuccessful(null);
            return $.ajax({ method: "Get",
                        url: this.coalesceConfig.baseApiUrl() + this.apiController + "/FullNameAndAge",
                        data: { id: this.myId,  },
                        xhrFields: { withCredentials: true } })
            .done((data) => {
                this.isDirty(false);
				this.fullNameAndAgeResultRaw(data.object);
                this.fullNameAndAgeMessage('');
                this.fullNameAndAgeWasSuccessful(true);
                this.fullNameAndAgeResult(data.object);

                if (typeof(callback) != "function") return;
                var result = this.fullNameAndAgeResult();
                if (reload) {
                  this.load(null, () => callback(result));
                } else {
                  callback(result);
                }
            })
            .fail((xhr) => {
                var errorMsg = "Unknown Error";
                if (xhr.responseJSON && xhr.responseJSON.message) errorMsg = xhr.responseJSON.message;
                this.fullNameAndAgeWasSuccessful(false);
                this.fullNameAndAgeMessage(errorMsg);
    
                if (this.coalesceConfig.showFailureAlerts())
                    this.coalesceConfig.onFailure()(this as any, "Could not call method fullNameAndAge: " + errorMsg);
            })
            .always(() => {
                this.fullNameAndAgeIsLoading(false);
            });
        } 
        /** Result of server method (FullNameAndAge) strongly typed in a observable. */
        public fullNameAndAgeResult: KnockoutObservable<string> = ko.observable(null);
        /** Raw result object of server method (FullNameAndAge) simply wrapped in an observable. */
        public fullNameAndAgeResultRaw: KnockoutObservable<any> = ko.observable();
        /** True while the server method (FullNameAndAge) is being called */
        public fullNameAndAgeIsLoading: KnockoutObservable<boolean> = ko.observable(false);
        /** Error message for server method (FullNameAndAge) if it fails. */
        public fullNameAndAgeMessage: KnockoutObservable<string> = ko.observable(null);
        /** True if the server method (FullNameAndAge) was successful. */
        public fullNameAndAgeWasSuccessful: KnockoutObservable<boolean> = ko.observable(null);
        /** Presents a series of input boxes to call the server method (FullNameAndAge) */
        public fullNameAndAgeUi = (callback: () => void = null, reload: boolean = true): JQueryPromise<any> => {
            var $promptVal: string = null;
            return this.fullNameAndAge(callback, reload);
        }
        /** Presents a modal with input boxes to call the server method (FullNameAndAge). Depends on a modal existing with id #method-FullNameAndAge. */
        public fullNameAndAgeModal = (callback: () => void = null, reload: boolean = true): void => {
            this.fullNameAndAgeUi(callback, reload);
        }
        
        
        
        /**
            Invoke server method ObfuscateEmail.
        */
        public obfuscateEmail = (callback: (result: string) => void = null, reload: boolean = true): JQueryPromise<any> => {

            this.obfuscateEmailIsLoading(true);
            this.obfuscateEmailMessage('');
            this.obfuscateEmailWasSuccessful(null);
            return $.ajax({ method: "Put",
                        url: this.coalesceConfig.baseApiUrl() + this.apiController + "/ObfuscateEmail",
                        data: { id: this.myId,  },
                        xhrFields: { withCredentials: true } })
            .done((data) => {
                this.isDirty(false);
				this.obfuscateEmailResultRaw(data.object);
                this.obfuscateEmailMessage('');
                this.obfuscateEmailWasSuccessful(true);
                this.obfuscateEmailResult(data.object);

                if (typeof(callback) != "function") return;
                var result = this.obfuscateEmailResult();
                if (reload) {
                  this.load(null, () => callback(result));
                } else {
                  callback(result);
                }
            })
            .fail((xhr) => {
                var errorMsg = "Unknown Error";
                if (xhr.responseJSON && xhr.responseJSON.message) errorMsg = xhr.responseJSON.message;
                this.obfuscateEmailWasSuccessful(false);
                this.obfuscateEmailMessage(errorMsg);
    
                if (this.coalesceConfig.showFailureAlerts())
                    this.coalesceConfig.onFailure()(this as any, "Could not call method obfuscateEmail: " + errorMsg);
            })
            .always(() => {
                this.obfuscateEmailIsLoading(false);
            });
        } 
        /** Result of server method (ObfuscateEmail) strongly typed in a observable. */
        public obfuscateEmailResult: KnockoutObservable<string> = ko.observable(null);
        /** Raw result object of server method (ObfuscateEmail) simply wrapped in an observable. */
        public obfuscateEmailResultRaw: KnockoutObservable<any> = ko.observable();
        /** True while the server method (ObfuscateEmail) is being called */
        public obfuscateEmailIsLoading: KnockoutObservable<boolean> = ko.observable(false);
        /** Error message for server method (ObfuscateEmail) if it fails. */
        public obfuscateEmailMessage: KnockoutObservable<string> = ko.observable(null);
        /** True if the server method (ObfuscateEmail) was successful. */
        public obfuscateEmailWasSuccessful: KnockoutObservable<boolean> = ko.observable(null);
        /** Presents a series of input boxes to call the server method (ObfuscateEmail) */
        public obfuscateEmailUi = (callback: () => void = null, reload: boolean = true): JQueryPromise<any> => {
            var $promptVal: string = null;
            return this.obfuscateEmail(callback, reload);
        }
        /** Presents a modal with input boxes to call the server method (ObfuscateEmail). Depends on a modal existing with id #method-ObfuscateEmail. */
        public obfuscateEmailModal = (callback: () => void = null, reload: boolean = true): void => {
            this.obfuscateEmailUi(callback, reload);
        }
        
        
        
        /**
            Invoke server method ChangeFirstName.
        */
        public changeFirstName = (firstName: string, callback: (result: ViewModels.Person) => void = null, reload: boolean = true): JQueryPromise<any> => {

            this.changeFirstNameIsLoading(true);
            this.changeFirstNameMessage('');
            this.changeFirstNameWasSuccessful(null);
            return $.ajax({ method: "Patch",
                        url: this.coalesceConfig.baseApiUrl() + this.apiController + "/ChangeFirstName",
                        data: { id: this.myId, firstName: firstName },
                        xhrFields: { withCredentials: true } })
            .done((data) => {
                this.isDirty(false);
				this.changeFirstNameResultRaw(data.object);
                this.changeFirstNameMessage('');
                this.changeFirstNameWasSuccessful(true);
                if (!this.changeFirstNameResult()){
                    this.changeFirstNameResult(new Person(data.object));
                } else {
                    this.changeFirstNameResult().loadFromDto(data.object);
                }

                // The return type is the type of the object, load it.
                this.loadFromDto(data.object, true)
                if (typeof(callback) == "function") {
                    var result = this.changeFirstNameResult();
                    callback(result);
                }
            })
            .fail((xhr) => {
                var errorMsg = "Unknown Error";
                if (xhr.responseJSON && xhr.responseJSON.message) errorMsg = xhr.responseJSON.message;
                this.changeFirstNameWasSuccessful(false);
                this.changeFirstNameMessage(errorMsg);
    
                if (this.coalesceConfig.showFailureAlerts())
                    this.coalesceConfig.onFailure()(this as any, "Could not call method changeFirstName: " + errorMsg);
            })
            .always(() => {
                this.changeFirstNameIsLoading(false);
            });
        } 
        /** Result of server method (ChangeFirstName) strongly typed in a observable. */
        public changeFirstNameResult: KnockoutObservable<ViewModels.Person> = ko.observable(null);
        /** Raw result object of server method (ChangeFirstName) simply wrapped in an observable. */
        public changeFirstNameResultRaw: KnockoutObservable<any> = ko.observable();
        /** True while the server method (ChangeFirstName) is being called */
        public changeFirstNameIsLoading: KnockoutObservable<boolean> = ko.observable(false);
        /** Error message for server method (ChangeFirstName) if it fails. */
        public changeFirstNameMessage: KnockoutObservable<string> = ko.observable(null);
        /** True if the server method (ChangeFirstName) was successful. */
        public changeFirstNameWasSuccessful: KnockoutObservable<boolean> = ko.observable(null);
        /** Presents a series of input boxes to call the server method (ChangeFirstName) */
        public changeFirstNameUi = (callback: () => void = null, reload: boolean = true): JQueryPromise<any> => {
            var $promptVal: string = null;
            $promptVal = prompt('First Name');
            if ($promptVal === null) return;
            var firstName: string = $promptVal;
              
            return this.changeFirstName(firstName, callback, reload);
        }
        /** Presents a modal with input boxes to call the server method (ChangeFirstName). Depends on a modal existing with id #method-ChangeFirstName. */
        public changeFirstNameModal = (callback: () => void = null, reload: boolean = true): void => {
            $('#method-ChangeFirstName').modal();
            $('#method-ChangeFirstName').on('shown.bs.modal', () => {
                $('#method-ChangeFirstName .btn-ok').unbind('click');
                $('#method-ChangeFirstName .btn-ok').click(() => {
                    this.changeFirstNameWithArgs(null, callback, reload);
                    $('#method-ChangeFirstName').modal('hide');
                });
            });
        }
        /** Calls server method (ChangeFirstName) with an instance of PersonPartial.ChangeFirstNameArgs, or the value of changeFirstNameArgs if not specified. */
        public changeFirstNameWithArgs = (args?: PersonPartial.ChangeFirstNameArgs, callback?: (result: ViewModels.Person) => void, reload: boolean = true) => {
            if (!args) args = this.changeFirstNameArgs;
            return this.changeFirstName(args.firstName(), callback, reload);
        }
        /** Object that can be easily bound to fields to allow data entry for the method */
        public changeFirstNameArgs = new PersonPartial.ChangeFirstNameArgs(); 
        

        /** 
            Load the ViewModel object from the DTO. 
            @param force: Will override the check against isLoading that is done to prevent recursion. False is default.
            @param allowCollectionDeletes: Set true when entire collections are loaded. True is the default. In some cases only a partial collection is returned, set to false to only add/update collections.
        */
        public loadFromDto = (data: any, force: boolean = false, allowCollectionDeletes: boolean = true): void => {
            if (!data || (!force && this.isLoading())) return;
            this.isLoading(true);
            // Set the ID 
            this.myId = data.personId;
            this.personId(data.personId);
            // Load the lists of other objects
            if (data.casesAssigned != null) {
                // Merge the incoming array
                Coalesce.KnockoutUtilities.RebuildArray(this.casesAssigned, data.casesAssigned, 'caseKey', Case, this, allowCollectionDeletes);
            } 
            if (data.casesReported != null) {
                // Merge the incoming array
                Coalesce.KnockoutUtilities.RebuildArray(this.casesReported, data.casesReported, 'caseKey', Case, this, allowCollectionDeletes);
            } 
            // Objects are loaded first so that they are available when the IDs get loaded.
            // This handles the issue with populating select lists with correct data because we now have the object.
            if (!data.personStats) { 
                this.personStats(null);
            } else {
                if (!this.personStats()){
                    this.personStats(new PersonStats(data.personStats, this));
                } else {
                    this.personStats().loadFromDto(data.personStats);
                }
            }
            if (!data.company) { 
                if (data.companyId != this.companyId()) {
                    this.company(null);
                }
            } else {
                if (!this.company()){
                    this.company(new Company(data.company, this));
                } else {
                    this.company().loadFromDto(data.company);
                }
                if (this.parent instanceof Company && this.parent !== this.company() && this.parent.companyId() == this.company().companyId())
                {
                    this.parent.loadFromDto(data.company, null, false);
                }
            }

            // The rest of the objects are loaded now.
            this.title(data.title);
            this.firstName(data.firstName);
            this.lastName(data.lastName);
            this.email(data.email);
            this.gender(data.gender);
            if (data.birthDate == null) this.birthDate(null);
            else if (this.birthDate() == null || this.birthDate().valueOf() != new Date(data.birthDate).getTime()){
                this.birthDate(moment(new Date(data.birthDate)));
            }
            if (data.lastBath == null) this.lastBath(null);
            else if (this.lastBath() == null || this.lastBath().valueOf() != new Date(data.lastBath).getTime()){
                this.lastBath(moment(new Date(data.lastBath)));
            }
            if (data.nextUpgrade == null) this.nextUpgrade(null);
            else if (this.nextUpgrade() == null || this.nextUpgrade().valueOf() != new Date(data.nextUpgrade).getTime()){
                this.nextUpgrade(moment(new Date(data.nextUpgrade)));
            }
            this.name(data.name);
            this.companyId(data.companyId);
            if (this.coalesceConfig.onLoadFromDto()){
                this.coalesceConfig.onLoadFromDto()(this as any);
            }
            this.isLoading(false);
            this.isDirty(false);
    
            if (this.coalesceConfig.validateOnLoadFromDto()) this.validate();
        };
    
        /** Saves this object into a data transfer object to send to the server. */
        public saveToDto = (): any => {
            var dto: any = {};
            dto.personId = this.personId();

            dto.title = this.title();
            dto.firstName = this.firstName();
            dto.lastName = this.lastName();
            dto.email = this.email();
            dto.gender = this.gender();
            if (!this.birthDate()) dto.birthDate = null;
            else dto.birthDate = this.birthDate().format('YYYY-MM-DDTHH:mm:ss');
            if (!this.lastBath()) dto.lastBath = null;
            else dto.lastBath = this.lastBath().format('YYYY-MM-DDTHH:mm:ss');
            if (!this.nextUpgrade()) dto.nextUpgrade = null;
            else dto.nextUpgrade = this.nextUpgrade().format('YYYY-MM-DDTHH:mm:ssZZ');
            dto.companyId = this.companyId();
            if (!dto.companyId && this.company()) {
                dto.companyId = this.company().companyId();
            }

            return dto;
        }
    
        /**
            Loads any child objects that have an ID set, but not the full object.
            This is useful when creating an object that has a parent object and the ID is set on the new child.
        */
        public loadChildren = (callback?: () => void): void => {
            var loadingCount = 0;
            // See if this.company needs to be loaded.
            if (this.company() == null && this.companyId() != null){
                loadingCount++;
                var companyObj = new Company();
                companyObj.load(this.companyId(), () => {
                    loadingCount--;
                    this.company(companyObj);
                    if (loadingCount == 0 && typeof(callback) == "function"){
                        callback();
                    }
                });
            }
            if (loadingCount == 0 && typeof(callback) == "function"){
                callback();
            }
        };
        
        public setupValidation = (): void => {
            if (this.errors !== null) return;
            this.errors = ko.validation.group([
                this.firstName.extend({ minLength: 2, maxLength: 75 }),
                this.lastName.extend({ minLength: 3, maxLength: 100 }),
                this.birthDate.extend({ moment: { unix: true } }),
                this.lastBath.extend({ moment: { unix: true } }),
                this.nextUpgrade.extend({ moment: { unix: true } }),
                this.companyId.extend({ required: {params: true, message: "Company is required."} }),
            ]);
            this.warnings = ko.validation.group([
            ]);
        }
    
        // Computed Observable for edit URL
        public editUrl: KnockoutComputed<string> = ko.pureComputed(() => {
            return this.coalesceConfig.baseViewUrl() + this.viewController + "/CreateEdit?id=" + this.personId();
        });

        constructor(newItem?: object, parent?: Coalesce.BaseViewModel | ListViewModels.PersonList){
            super(parent);
            this.baseInitialize();
            var self = this;
            self.myId;

            // Create computeds for display for objects
			self.personStatsText = ko.pureComputed(function()
			{   // If the object exists, use the text value. Otherwise show 'None'
				if (self.personStats() && self.personStats().name()) {
					return self.personStats().name().toString();
				} else {
					return "None";
				}
			});
			self.companyText = ko.pureComputed(function()
			{   // If the object exists, use the text value. Otherwise show 'None'
				if (self.company() && self.company().altName()) {
					return self.company().altName().toString();
				} else {
					return "None";
				}
			});

    
            // List Object model for CasesAssigned. Allows for loading subsets of data.
            var _casesAssignedList: ListViewModels.CaseList = null;
            self.casesAssignedList = function(loadImmediate = true) {
                if (!_casesAssignedList){
                    _casesAssignedList = new ListViewModels.CaseList();
                    if (loadImmediate) loadCasesAssignedList();
                    self.personId.subscribe(loadCasesAssignedList)
                }
                return _casesAssignedList;
            }
            function loadCasesAssignedList() {
                if (self.personId()){
                    _casesAssignedList.queryString = "filter.AssignedToId=" + self.personId();
                    _casesAssignedList.load();
                }
            }
            // List Object model for CasesReported. Allows for loading subsets of data.
            var _casesReportedList: ListViewModels.CaseList = null;
            self.casesReportedList = function(loadImmediate = true) {
                if (!_casesReportedList){
                    _casesReportedList = new ListViewModels.CaseList();
                    if (loadImmediate) loadCasesReportedList();
                    self.personId.subscribe(loadCasesReportedList)
                }
                return _casesReportedList;
            }
            function loadCasesReportedList() {
                if (self.personId()){
                    _casesReportedList.queryString = "filter.ReportedById=" + self.personId();
                    _casesReportedList.load();
                }
            }


            self.showCompanyEditor = function(callback: any) {
                if (!self.company()) {
                    self.company(new Company());
                }
                self.company().showEditor(callback)
            };

            // This stuff needs to be done after everything else is set up.
            self.title.subscribe(self.autoSave);
            self.firstName.subscribe(self.autoSave);
            self.lastName.subscribe(self.autoSave);
            self.email.subscribe(self.autoSave);
            self.gender.subscribe(self.autoSave);
            self.birthDate.subscribe(self.autoSave);
            self.lastBath.subscribe(self.autoSave);
            self.nextUpgrade.subscribe(self.autoSave);
            self.companyId.subscribe(self.autoSave);
            self.company.subscribe(self.autoSave);
        
            if (newItem) {
                self.loadFromDto(newItem, true);
            }
        }
    }





    export namespace PersonPartial {
        export enum TitleEnum {
            Mr = 0,
            Ms = 1,
            Mrs = 2,
            Miss = 4,
        };
        export enum GenderEnum {
            NonSpecified = 0,
            Male = 1,
            Female = 2,
        };

        // Classes for use in method calls to support data binding for input for arguments
        export class RenameArgs {
            public name: KnockoutObservable<string> = ko.observable(null);
        }
        export class ChangeFirstNameArgs {
            public firstName: KnockoutObservable<string> = ko.observable(null);
        }
    }
}