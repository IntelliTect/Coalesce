/// <reference path="../../typings/tsd.d.ts" />
/// <reference path="../Coalesce/intellitect.utilities.ts" />
/// <reference path="../Coalesce/intellitect.ko.utilities.ts" />
/// <reference path="../Coalesce/intellitect.ko.base.ts" />



// Knockout View Model for: CaseProduct
// Auto Generated Knockout Object Bindings
// Copyright IntelliTect, 2017

var baseUrl = baseUrl || '';
var saveTimeoutInMs = saveTimeoutInMs || 500;

module ViewModels {

	export class CaseProduct extends BaseViewModel<CaseProduct>
    {
        protected modelName = "CaseProduct";
        protected modelDisplayName = "Case Product";
        protected areaUrl = ((true) ? baseUrl : baseUrl + '/');
        protected primaryKeyName = "caseProductId";
        protected apiUrlBase = "api/CaseProduct";
        protected viewUrlBase = "CaseProduct";
        public dataSources = ListViewModels.CaseProductDataSources;


        // The custom code to run in order to pull the initial datasource to use for the object that should be returned
        public dataSource: ListViewModels.CaseProductDataSources = ListViewModels.CaseProductDataSources.Default;
        
    
        // Observables
        public caseProductId: KnockoutObservable<number> = ko.observable(null);
        public caseId: KnockoutObservable<number> = ko.observable(null);
        public case: KnockoutObservable<ViewModels.Case> = ko.observable(null);
        public productId: KnockoutObservable<number> = ko.observable(null);
        public product: KnockoutObservable<ViewModels.Product> = ko.observable(null);

       
        // Create computeds for display for objects
        public caseText: () => string;
        public productText: () => string;
        

                public caseValidValues: KnockoutObservableArray<any> = ko.observableArray([]);
        public loadCaseValidValues: (callback?: any) => void;
        public productValidValues: KnockoutObservableArray<any> = ko.observableArray([]);
        public loadProductValidValues: (callback?: any) => void;
        // Pops up a stock editor for this object.
        public showEditor: () => void;
        public showCaseEditor: () => void;
        public showProductEditor: () => void;



        
        public originalData: KnockoutObservable<any> = ko.observable(null);
        
        // This method gets called during the constructor. This allows injecting new methods into the class that use the self variable.
        public init(myself: CaseProduct) {};

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
			self.caseId = self.caseId.extend({ required: {params: true, message: "Case is required."} });
			self.productId = self.productId.extend({ required: {params: true, message: "Product is required."} });
            
            self.errors = ko.validation.group([
                self.caseProductId,
                self.caseId,
                self.case,
                self.productId,
                self.product,
            ]);
            self.warnings = ko.validation.group([
            ]);

            // Computed Observable for edit URL
            self.editUrl = ko.computed(function() {
                return self.areaUrl + self.viewUrlBase + "/CreateEdit?id=" + self.caseProductId();
            });

            // Create computeds for display for objects
			self.caseText = ko.computed(function()
			{   // If the object exists, use the text value. Otherwise show 'None'
				if (self.case() && self.case().caseKey()) {
					return self.case().caseKey().toString();
				} else {
					return "None";
				}
			});
			self.productText = ko.computed(function()
			{   // If the object exists, use the text value. Otherwise show 'None'
				if (self.product() && self.product().name()) {
					return self.product().name().toString();
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
				self.myId = data.caseProductId;
				// Load the lists of other objects
				// Objects are loaded first so that they are available when the IDs get loaded.
				// This handles the issue with populating select lists with correct data because we now have the object.
				if (!data.case) { 
					if (data.caseId != self.caseId()) {
                        self.case(null);
                    }
                }else {
                    if (!self.case()){
					    self.case(new Case(data.case, self));
				    }else{
					    self.case().loadFromDto(data.case);
				    }
                    if (self.parent && self.parent.myId == self.case().myId && intellitect.utilities.getClassName(self.parent) == intellitect.utilities.getClassName(self.case()))
                    {
                        self.parent.loadFromDto(data.case, undefined, false);
                    }
                }
				if (!data.product) { 
					if (data.productId != self.productId()) {
                        self.product(null);
                    }
                }else {
                    if (!self.product()){
					    self.product(new Product(data.product, self));
				    }else{
					    self.product().loadFromDto(data.product);
				    }
                    if (self.parent && self.parent.myId == self.product().myId && intellitect.utilities.getClassName(self.parent) == intellitect.utilities.getClassName(self.product()))
                    {
                        self.parent.loadFromDto(data.product, undefined, false);
                    }
                }

				// The rest of the objects are loaded now.
				self.caseProductId(data.caseProductId);
				self.caseId(data.caseId);
				self.productId(data.productId);
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
				dto.caseProductId = self.caseProductId();

				dto.caseId = self.caseId();
				if (!dto.caseId && self.case()) {
				    dto.caseId = self.case().caseKey();
				}
				dto.productId = self.productId();
				if (!dto.productId && self.product()) {
				    dto.productId = self.product().productId();
				}

				return dto;
			}

            // Methods to add to child collections


            // Save on changes
            function setupSubscriptions() {
                self.caseId.subscribe(self.autoSave);
                self.case.subscribe(self.autoSave);
                self.productId.subscribe(self.autoSave);
                self.product.subscribe(self.autoSave);
            }  

            // Create variables for ListEditorApiUrls
            // Create loading function for Valid Values

            self.loadCaseValidValues = function(callback) {
                self.loadingValidValues++;
                $.ajax({ method: "GET", url: self.areaUrl + "api/Case/CustomList?Fields=CaseKey,CaseKey", xhrFields: { withCredentials: true } })
                .done(function(data) {
                    self.isLoading(true);
                    self.caseValidValues(data.list);
                    self.isLoading(false);
                })
                .fail(function(xhr) {
                    var errorMsg = "Unknown Error";
                    if (xhr.responseJSON && xhr.responseJSON.message) errorMsg = xhr.responseJSON.message;
                    self.isLoading(false);

                    if (self.showFailureAlerts)
                        alert("Could not get Valid Values for Case: " + errorMsg);
                })
                .always(function(){
                    self.loadingValidValues--;
                    if (self.loadingValidValues === 0) {
                        if ($.isFunction(callback)) {callback();}
                    }
                });
            }
            
            self.loadProductValidValues = function(callback) {
                self.loadingValidValues++;
                $.ajax({ method: "GET", url: self.areaUrl + "api/Product/CustomList?Fields=ProductId,Name", xhrFields: { withCredentials: true } })
                .done(function(data) {
                    self.isLoading(true);
                    self.productValidValues(data.list);
                    self.isLoading(false);
                })
                .fail(function(xhr) {
                    var errorMsg = "Unknown Error";
                    if (xhr.responseJSON && xhr.responseJSON.message) errorMsg = xhr.responseJSON.message;
                    self.isLoading(false);

                    if (self.showFailureAlerts)
                        alert("Could not get Valid Values for Product: " + errorMsg);
                })
                .always(function(){
                    self.loadingValidValues--;
                    if (self.loadingValidValues === 0) {
                        if ($.isFunction(callback)) {callback();}
                    }
                });
            }
            
            self.showCaseEditor = function() {
                if (!self.case()) {
                    self.case(new Case());
                }
                self.case().showEditor()
            };
            self.showProductEditor = function() {
                if (!self.product()) {
                    self.product(new Product());
                }
                self.product().showEditor()
            };

            // Load all child objects that are not loaded.
            self.loadChildren = function(callback) {
                var loadingCount = 0;
                var obj;
                // See if self.case needs to be loaded.
                if (self.case() == null && self.caseId() != null){
                    loadingCount++;
                    obj = new Case();
                    obj.load(self.caseId(), function() {
                        loadingCount--;
                        self.case(obj);
                        if (loadingCount == 0 && $.isFunction(callback)){
                            callback();
                        }
                    });
                }
                // See if self.product needs to be loaded.
                if (self.product() == null && self.productId() != null){
                    loadingCount++;
                    obj = new Product();
                    obj.load(self.productId(), function() {
                        loadingCount--;
                        self.product(obj);
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
                self.loadCaseValidValues(callback);
                self.loadProductValidValues(callback);
            };

            // Enumeration Lookups.


            // Method Implementations


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





    export namespace CaseProduct {

        // Classes for use in method calls to support data binding for input for arguments
    }
}