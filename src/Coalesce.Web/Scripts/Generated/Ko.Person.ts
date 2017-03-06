/// <reference path="../../typings/tsd.d.ts" />
/// <reference path="../Coalesce/intellitect.utilities.ts" />
/// <reference path="../Coalesce/intellitect.ko.utilities.ts" />
/// <reference path="../Coalesce/intellitect.ko.base.ts" />



// Knockout View Model for: Person
// Auto Generated Knockout Object Bindings
// Copyright IntelliTect, 2017

var baseUrl = baseUrl || '';
var saveTimeoutInMs = saveTimeoutInMs || 500;

module ViewModels {

	export class Person extends BaseViewModel<Person>
    {
        protected modelName = "Person";
        protected modelDisplayName = "Person";
        protected areaUrl = ((true) ? baseUrl : baseUrl + '/');
        protected primaryKeyName = "personId";
        protected apiUrlBase = "api/Person";
        protected viewUrlBase = "Person";
        public dataSources = ListViewModels.PersonDataSources;


        // The custom code to run in order to pull the initial datasource to use for the object that should be returned
        public dataSource: ListViewModels.PersonDataSources = ListViewModels.PersonDataSources.Default;
        
    
        // Observables
        // ID for the person object.
        public personId: KnockoutObservable<number> = ko.observable(null);
        // Title of the person, Mr. Mrs, etc.
        public title: KnockoutObservable<number> = ko.observable(null);
        // Text value for enumeration Title
        // Title of the person, Mr. Mrs, etc.
        public titleText: KnockoutComputed<string> = ko.computed<string>(() => "");
        // First name of the person.
        public firstName: KnockoutObservable<string> = ko.observable(null);
        // Last name of the person
        public lastName: KnockoutObservable<string> = ko.observable(null);
        // Email address of the person
        public email: KnockoutObservable<string> = ko.observable(null);
        // Genetic Gender of the person.
        public gender: KnockoutObservable<number> = ko.observable(null);
        // Text value for enumeration Gender
        // Genetic Gender of the person.
        public genderText: KnockoutComputed<string> = ko.computed<string>(() => "");
        // List of cases assigned to the person
        public casesAssigned: KnockoutObservableArray<ViewModels.Case> = ko.observableArray([]);
        // List of cases reported by the person.
        public casesReported: KnockoutObservableArray<ViewModels.Case> = ko.observableArray([]);
        public birthDate: KnockoutObservable<any> = ko.observable(null);
        public lastBath: KnockoutObservable<any> = ko.observable(null);
        public nextUpgrade: KnockoutObservable<any> = ko.observable(null);
        public personStatsId: KnockoutObservable<number> = ko.observable(null);
        public personStats: KnockoutObservable<ViewModels.PersonStats> = ko.observable(null);
        public timeZone: KnockoutObservable<any> = ko.observable(null);
        // Calculated name of the person. eg., Mr. Michael Stokesbary.
        public name: KnockoutObservable<string> = ko.observable(null);
        // Company ID this person is employed by
        public companyId: KnockoutObservable<number> = ko.observable(null);
        // Company loaded from the Company ID
        public company: KnockoutObservable<ViewModels.Company> = ko.observable(null);

       
        // Create computeds for display for objects
        public personStatsText: () => string;
        // Company loaded from the Company ID
        public companyText: () => string;
        
        // Add object to casesAssigned
        public addToCasesAssigned: () => Case;
        // List Object model for CasesAssigned. Allows for loading subsets of data.
        public casesAssignedList: (loadImmediate?: boolean) => ListViewModels.CaseList;
        // Add object to casesReported
        public addToCasesReported: () => Case;
        // List Object model for CasesReported. Allows for loading subsets of data.
        public casesReportedList: (loadImmediate?: boolean) => ListViewModels.CaseList;

        // List of cases assigned to the person
        public CasesAssignedListUrl: () => void; 
        // List of cases reported by the person.
        public CasesReportedListUrl: () => void; 
                // Company loaded from the Company ID
        public companyValidValues: KnockoutObservableArray<any> = ko.observableArray([]);
        public loadCompanyValidValues: (callback?: any) => void;
        // Pops up a stock editor for this object.
        public showEditor: (callback?: any) => void;
        public showPersonStatsEditor: (callback?: any) => void;
        public showCompanyEditor: (callback?: any) => void;


        public titleValues: EnumValue[] = [ 
            { id: 0, value: 'Mr' },
            { id: 1, value: 'Ms' },
            { id: 2, value: 'Mrs' },
            { id: 4, value: 'Miss' },
        ];
        public genderValues: EnumValue[] = [ 
            { id: 0, value: 'Non Specified' },
            { id: 1, value: 'Male' },
            { id: 2, value: 'Female' },
        ];

        // Call server method (Rename)
        // Adds the text to the first name.
        public rename: (addition: String, callback?: any, reload?: boolean) => void;
        // Result of server method (Rename)
        public renameResult: KnockoutObservable<ViewModels.Person> = ko.observable(null);
        // True while the server method (Rename) is being called
        public renameIsLoading: KnockoutObservable<boolean> = ko.observable(false);
        // Error message for server method (Rename) if it fails.
        public renameMessage: KnockoutObservable<string> = ko.observable(null);
        // True if the server method (Rename) was successful.
        public renameWasSuccessful: KnockoutObservable<boolean> = ko.observable(null);
        // Presents a series of input boxes to call the server method (Rename)
        public renameUi: (callback?: any, reload?: boolean) => void;
        // Presents a modal with input boxes to call the server method (Rename)
        public renameModal: (callback?: any, reload?: boolean) => void;
        // Variable for method arguments to allow for easy binding
        public renameWithArgs: (args?: Person.RenameArgs, callback?: any, reload?: boolean) => void;
        // Object that can be easily bound to fields to allow data entry for the method
        public renameArgs = new Person.RenameArgs(); 
        
        // Call server method (ChangeSpacesToDashesInName)
        // Removes spaces from the name and puts in dashes
        public changeSpacesToDashesInName: (callback?: any, reload?: boolean) => void;
        // Result of server method (ChangeSpacesToDashesInName)
        public changeSpacesToDashesInNameResult: KnockoutObservable<any> = ko.observable(null);
        // True while the server method (ChangeSpacesToDashesInName) is being called
        public changeSpacesToDashesInNameIsLoading: KnockoutObservable<boolean> = ko.observable(false);
        // Error message for server method (ChangeSpacesToDashesInName) if it fails.
        public changeSpacesToDashesInNameMessage: KnockoutObservable<string> = ko.observable(null);
        // True if the server method (ChangeSpacesToDashesInName) was successful.
        public changeSpacesToDashesInNameWasSuccessful: KnockoutObservable<boolean> = ko.observable(null);
        // Presents a series of input boxes to call the server method (ChangeSpacesToDashesInName)
        public changeSpacesToDashesInNameUi: (callback?: any, reload?: boolean) => void;
        // Presents a modal with input boxes to call the server method (ChangeSpacesToDashesInName)
        public changeSpacesToDashesInNameModal: (callback?: any, reload?: boolean) => void;
        // Variable for method arguments to allow for easy binding
        
        
        public originalData: KnockoutObservable<any> = ko.observable(null);
        
        // This method gets called during the constructor. This allows injecting new methods into the class that use the self variable.
        public init(myself: Person) {};

        constructor(newItem?: any, parent?: any){
            super();
            var self = this;
            self.parent = parent;
            self.myId;
            // Call an init function that allows for proper inheritance.
            if ($.isFunction(self.init)){
                self.init(self);
            }
            
            ko.validation.init({
                grouping: {
                    deep: true,
                    live: true,
                    observable: true
                }
            });

            // SetupValidation {
			self.firstName = self.firstName.extend({ minLength: 2 });
			self.lastName = self.lastName.extend({ minLength: 3 });
			self.birthDate = self.birthDate.extend({ moment: { unix: true } });
			self.lastBath = self.lastBath.extend({ moment: { unix: true } });
			self.nextUpgrade = self.nextUpgrade.extend({ moment: { unix: true } });
			self.companyId = self.companyId.extend({ required: {params: true, message: "Company is required."} });
            self.companyId = self.companyId.extend({ required: true });
            
            self.errors = ko.validation.group([
                self.personId,
                self.title,
                self.firstName,
                self.lastName,
                self.email,
                self.gender,
                self.casesAssigned,
                self.casesReported,
                self.birthDate,
                self.lastBath,
                self.nextUpgrade,
                self.personStatsId,
                self.personStats,
                self.timeZone,
                self.name,
                self.companyId,
                self.company,
            ]);
            self.warnings = ko.validation.group([
            ]);

            // Computed Observable for edit URL
            self.editUrl = ko.computed(function() {
                return self.areaUrl + self.viewUrlBase + "/CreateEdit?id=" + self.personId();
            });

            // Create computeds for display for objects
			self.personStatsText = ko.computed(function()
			{   // If the object exists, use the text value. Otherwise show 'None'
				if (self.personStats() && self.personStats().personStatsId()) {
					return self.personStats().personStatsId().toString();
				} else {
					return "None";
				}
			});
			self.companyText = ko.computed(function()
			{   // If the object exists, use the text value. Otherwise show 'None'
				if (self.company() && self.company().altName()) {
					return self.company().altName().toString();
				} else {
					return "None";
				}
			});


            // Load the ViewModel object from the DTO. 
            // Force: Will override the check against isLoading that is done to prevent recursion. False is default.
            // AllowCollectionDeletes: Set true when entire collections are loaded. True is the default. In some cases only a partial collection is returned, set to false to only add/update collections.
			self.loadFromDto = function(data: any, force: boolean = false, allowCollectionDeletes: boolean = true) {
				if (!data || (!force && self.isLoading())) return;
				self.isLoading(true);
				// Set the ID 
				self.myId = data.personId;
				// Load the lists of other objects
                if (data.casesAssigned != null) {
				    // Merge the incoming array
				    RebuildArray(self.casesAssigned, data.casesAssigned, 'caseKey', Case, self, allowCollectionDeletes);
				} 
                if (data.casesReported != null) {
				    // Merge the incoming array
				    RebuildArray(self.casesReported, data.casesReported, 'caseKey', Case, self, allowCollectionDeletes);
				} 
				// Objects are loaded first so that they are available when the IDs get loaded.
				// This handles the issue with populating select lists with correct data because we now have the object.
				if (!data.personStats) { 
					if (data.personStatsId != self.personStatsId()) {
                        self.personStats(null);
                    }
                }else {
                    if (!self.personStats()){
					    self.personStats(new PersonStats(data.personStats, self));
				    }else{
					    self.personStats().loadFromDto(data.personStats);
				    }
                }
				if (!data.company) { 
					if (data.companyId != self.companyId()) {
                        self.company(null);
                    }
                }else {
                    if (!self.company()){
					    self.company(new Company(data.company, self));
				    }else{
					    self.company().loadFromDto(data.company);
				    }
                    if (self.parent && self.parent.myId == self.company().myId && intellitect.utilities.getClassName(self.parent) == intellitect.utilities.getClassName(self.company()))
                    {
                        self.parent.loadFromDto(data.company, undefined, false);
                    }
                }

				// The rest of the objects are loaded now.
				self.personId(data.personId);
				self.title(data.title);
				self.firstName(data.firstName);
				self.lastName(data.lastName);
				self.email(data.email);
				self.gender(data.gender);
                if (data.birthDate == null) self.birthDate(null);
				else if (self.birthDate() == null || self.birthDate() == false || !self.birthDate().isSame(moment(data.birthDate))){
				    self.birthDate(moment(data.birthDate));
				}
                if (data.lastBath == null) self.lastBath(null);
				else if (self.lastBath() == null || self.lastBath() == false || !self.lastBath().isSame(moment(data.lastBath))){
				    self.lastBath(moment(data.lastBath));
				}
                if (data.nextUpgrade == null) self.nextUpgrade(null);
				else if (self.nextUpgrade() == null || self.nextUpgrade() == false || !self.nextUpgrade().isSame(moment(data.nextUpgrade))){
				    self.nextUpgrade(moment(data.nextUpgrade));
				}
				self.personStatsId(data.personStatsId);
				self.timeZone(data.timeZone);
				self.name(data.name);
				self.companyId(data.companyId);
                if (self.afterLoadFromDto){
                    self.afterLoadFromDto();
                }
				self.isLoading(false);
				self.isDirty(false);
                self.validate();
			};

    	    // Save the object into a DTO
			self.saveToDto = function() {
				var dto: any = {};
				dto.personId = self.personId();

    	        dto.title = self.title();
    	        dto.firstName = self.firstName();
    	        dto.lastName = self.lastName();
    	        dto.email = self.email();
    	        dto.gender = self.gender();
                if (!self.birthDate()) dto.BirthDate = null;
				else dto.birthDate = self.birthDate().format('YYYY-MM-DDTHH:mm:ss');
                if (!self.lastBath()) dto.LastBath = null;
				else dto.lastBath = self.lastBath().format('YYYY-MM-DDTHH:mm:ss');
				if (!self.nextUpgrade()) dto.NextUpgrade = null;
				else dto.nextUpgrade = self.nextUpgrade().format('YYYY-MM-DDTHH:mm:ssZZ');
				dto.personStatsId = self.personStatsId();
				if (!dto.personStatsId && self.personStats()) {
				    dto.personStatsId = self.personStats().personStatsId();
				}
    	        dto.timeZone = self.timeZone();
				dto.companyId = self.companyId();
				if (!dto.companyId && self.company()) {
				    dto.companyId = self.company().companyId();
				}

				return dto;
			}

            // Methods to add to child collections

            self.addToCasesAssigned = function() {
                var newItem = new Case();
                newItem.parent = self;
                newItem.parentCollection = self.casesAssigned;
                newItem.isExpanded(true);
                newItem.assignedToId(self.personId());
                self.casesAssigned.push(newItem);
                return newItem;
            }
            
            self.addToCasesReported = function() {
                var newItem = new Case();
                newItem.parent = self;
                newItem.parentCollection = self.casesReported;
                newItem.isExpanded(true);
                newItem.reportedById(self.personId());
                self.casesReported.push(newItem);
                return newItem;
            }
            
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
                    _casesAssignedList.queryString = "AssignedToId=" + self.personId();
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
                    _casesReportedList.queryString = "ReportedById=" + self.personId();
                    _casesReportedList.load();
                }
            }

            // Save on changes
            function setupSubscriptions() {
                self.title.subscribe(self.autoSave);
                self.firstName.subscribe(self.autoSave);
                self.lastName.subscribe(self.autoSave);
                self.email.subscribe(self.autoSave);
                self.gender.subscribe(self.autoSave);
                self.birthDate.subscribe(self.autoSave);
                self.lastBath.subscribe(self.autoSave);
                self.nextUpgrade.subscribe(self.autoSave);
                self.personStatsId.subscribe(self.autoSave);
                self.timeZone.subscribe(self.autoSave);
                self.companyId.subscribe(self.autoSave);
                self.company.subscribe(self.autoSave);
            }  

            // Create variables for ListEditorApiUrls
            self.CasesAssignedListUrl = ko.computed({
                read: function() {
                         return self.areaUrl + 'Case/table?assignedToId=' + self.personId();
                },
                deferEvaluation: true
            });
            self.CasesReportedListUrl = ko.computed({
                read: function() {
                         return self.areaUrl + 'Case/table?reportedById=' + self.personId();
                },
                deferEvaluation: true
            });
            // Create loading function for Valid Values

            self.loadCompanyValidValues = function(callback) {
                self.loadingValidValues++;
                $.ajax({ method: "GET", url: self.areaUrl + "api/Company/CustomList?Fields=CompanyId,AltName", xhrFields: { withCredentials: true } })
                .done(function(data) {
                    self.isLoading(true);
                    self.companyValidValues(data.list);
                    self.isLoading(false);
                })
                .fail(function(xhr) {
                    var errorMsg = "Unknown Error";
                    if (xhr.responseJSON && xhr.responseJSON.message) errorMsg = xhr.responseJSON.message;
                    self.isLoading(false);

                    if (self.showFailureAlerts)
                        alert("Could not get Valid Values for Company: " + errorMsg);
                })
                .always(function(){
                    self.loadingValidValues--;
                    if (self.loadingValidValues === 0) {
                        if ($.isFunction(callback)) {callback();}
                    }
                });
            }
            
            self.showCompanyEditor = function(callback: any) {
                if (!self.company()) {
                    self.company(new Company());
                }
                self.company().showEditor(callback)
            };

            // Load all child objects that are not loaded.
            self.loadChildren = function(callback) {
                var loadingCount = 0;
                var obj;
                // See if self.company needs to be loaded.
                if (self.company() == null && self.companyId() != null){
                    loadingCount++;
                    obj = new Company();
                    obj.load(self.companyId(), function() {
                        loadingCount--;
                        self.company(obj);
                        if (loadingCount == 0 && $.isFunction(callback)){
                            callback();
                        }
                    });
                }
                if (loadingCount == 0 && $.isFunction(callback)){
                    callback();
                }
            };



            // Load all the valid values in parallel.
            self.loadValidValues = function(callback) {
                self.loadingValidValues = 0;
                self.loadCompanyValidValues(callback);
            };

            // Enumeration Lookups.
            self.titleText = ko.computed(function() {
                for(var i=0;i < self.titleValues.length; i++){
                    if (self.titleValues[i].id == self.title()){
                        return self.titleValues[i].value;
                    }
                }
            });
            self.genderText = ko.computed(function() {
                for(var i=0;i < self.genderValues.length; i++){
                    if (self.genderValues[i].id == self.gender()){
                        return self.genderValues[i].value;
                    }
                }
            });


            // Method Implementations

            self.rename = function(addition: String, callback?: any, reload: boolean = true){
                self.renameIsLoading(true);
                $.ajax({ method: "POST",
                         url: self.areaUrl + "api/Person/Rename",
                         data: {
                        id: self.myId, 
                        addition: addition
                    },
                         xhrFields: { withCredentials: true } })
                .done(function(data) {
                    self.isDirty(false);
                    self.renameMessage('');
                    self.renameWasSuccessful(true);
                    if (!self.renameResult()){
                        self.renameResult(new Person(data.object));
                    }else{
                        self.renameResult().loadFromDto(data.object);
                    }

                    // The return type is the type of the object, load it.
                    self.loadFromDto(data.object, true)
                    if ($.isFunction(callback)) {
                        callback();
                    }
                })
                .fail(function(xhr) {
                    var errorMsg = "Unknown Error";
                    if (xhr.responseJSON && xhr.responseJSON.message) errorMsg = xhr.responseJSON.message;
                    self.renameWasSuccessful(false);
                    self.renameMessage(errorMsg);

                    if (self.showFailureAlerts)
                        alert("Could not call method rename: " + errorMsg);
                })
                .always(function() {
                    self.renameIsLoading(false);
                });
            }
            
            self.renameUi = function(callback?: any, reload: boolean = true) {
                var addition: String = prompt('Addition');
                self.rename(addition, callback, reload);
            }
            self.renameModal = function(callback?: any, reload: boolean = true) {
                $('#method-Rename').modal();
                $('#method-Rename').on('shown.bs.modal', function() {
                    $('#method-Rename .btn-ok').unbind('click');
                    $('#method-Rename .btn-ok').click(function()
                    {
                        self.renameWithArgs(null, callback, reload);
                        $('#method-Rename').modal('hide');
                    });
                });
            }
            self.renameWithArgs = function(args?: Person.RenameArgs, callback?: any, reload: boolean = true) {
                if (!args) args = self.renameArgs;
                self.rename(args.addition(), callback, reload);
            }

            self.changeSpacesToDashesInName = function(callback?: any, reload: boolean = true){
                self.changeSpacesToDashesInNameIsLoading(true);
                $.ajax({ method: "POST",
                         url: self.areaUrl + "api/Person/ChangeSpacesToDashesInName",
                         data: {
                        id: self.myId
                    },
                         xhrFields: { withCredentials: true } })
                .done(function(data) {
                    self.isDirty(false);
                    self.changeSpacesToDashesInNameMessage('');
                    self.changeSpacesToDashesInNameWasSuccessful(true);
                    self.changeSpacesToDashesInNameResult(data.object);

                    if (reload) {
                      self.reload(callback);
                    } else if ($.isFunction(callback)) {
                      callback(data);
                    }
                })
                .fail(function(xhr) {
                    var errorMsg = "Unknown Error";
                    if (xhr.responseJSON && xhr.responseJSON.message) errorMsg = xhr.responseJSON.message;
                    self.changeSpacesToDashesInNameWasSuccessful(false);
                    self.changeSpacesToDashesInNameMessage(errorMsg);

                    if (self.showFailureAlerts)
                        alert("Could not call method changeSpacesToDashesInName: " + errorMsg);
                })
                .always(function() {
                    self.changeSpacesToDashesInNameIsLoading(false);
                });
            }
            
            self.changeSpacesToDashesInNameUi = function(callback?: any, reload: boolean = true) {
                self.changeSpacesToDashesInName(callback, reload);
            }
            self.changeSpacesToDashesInNameModal = function(callback?: any, reload: boolean = true) {
                    self.changeSpacesToDashesInNameUi(callback, reload);
            }


            // This stuff needs to be done after everything else is set up.
            // Complex Type Observables

            // Make sure everything is defined before we call this.
            setupSubscriptions();

            if (newItem) {
                if ($.isNumeric(newItem)) self.load(newItem);
                else self.loadFromDto(newItem, true);
            }



        }
    }





    export namespace Person {
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
            public addition: KnockoutObservable<string> = ko.observable(null);
        }
    }
}