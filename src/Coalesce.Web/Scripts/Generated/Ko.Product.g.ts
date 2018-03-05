
/// <reference path="../coalesce.dependencies.d.ts" />

// Generated by IntelliTect.Coalesce

module ViewModels {
    
    export class Product extends Coalesce.BaseViewModel {
        public readonly modelName = "Product";
        public readonly primaryKeyName: keyof this = "productId";
        public readonly modelDisplayName = "Product";
        public readonly apiController = "/Product";
        public readonly viewController = "/Product";
        
        /** Behavioral configuration for all instances of Product. Can be overidden on each instance via instance.coalesceConfig. */
        public static coalesceConfig: Coalesce.ViewModelConfiguration<Product>
            = new Coalesce.ViewModelConfiguration<Product>(Coalesce.GlobalConfiguration.viewModel);
        
        /** Behavioral configuration for the current Product instance. */
        public coalesceConfig: Coalesce.ViewModelConfiguration<this>
            = new Coalesce.ViewModelConfiguration<Product>(Product.coalesceConfig);
        
        /** The namespace containing all possible values of this.dataSource. */
        public dataSources: typeof ListViewModels.ProductDataSources = ListViewModels.ProductDataSources;
        
        
        public productId: KnockoutObservable<number | null> = ko.observable(null);
        public name: KnockoutObservable<string | null> = ko.observable(null);
        public details: KnockoutObservable<ViewModels.ProductDetails | null> = ko.observable(null);
        
        
        /** Display text for Details */
        public detailsText: KnockoutComputed<string>;
        
        
        
        
        
        
        /** 
            Load the ViewModel object from the DTO.
            @param force: Will override the check against isLoading that is done to prevent recursion. False is default.
            @param allowCollectionDeletes: Set true when entire collections are loaded. True is the default. 
            In some cases only a partial collection is returned, set to false to only add/update collections.
        */
        public loadFromDto = (data: any, force: boolean = false, allowCollectionDeletes: boolean = true): void => {
            if (!data || (!force && this.isLoading())) return;
            this.isLoading(true);
            // Set the ID 
            this.myId = data.productId;
            this.productId(data.productId);
            // Load the lists of other objects
            if (!data.details) { 
                this.details(null);
            } else {
                if (!this.details()){
                    this.details(new ProductDetails(data.details, this));
                } else {
                    this.details()!.loadFromDto(data.details);
                }
            }
            
            // The rest of the objects are loaded now.
            this.name(data.name);
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
            dto.productId = this.productId();
            
            dto.name = this.name();
            
            return dto;
        }
        
        /** 
            Loads any child objects that have an ID set, but not the full object.
            This is useful when creating an object that has a parent object and the ID is set on the new child.
        */
        public loadChildren = (callback?: () => void): void => {
            var loadingCount = 0;
            if (loadingCount == 0 && typeof(callback) == "function") { callback(); }
        };
        
        public setupValidation(): void {
            if (this.errors !== null) return;
            this.errors = ko.validation.group([
            ]);
            this.warnings = ko.validation.group([
            ]);
        }
        
        constructor(newItem?: object, parent?: Coalesce.BaseViewModel | ListViewModels.ProductList) {
            super(parent);
            this.baseInitialize();
            const self = this;
            
            this.detailsText = ko.pureComputed(function() {
                if (self.details() && self.details()!.manufacturingAddress()) {
                    return self.details()!.manufacturingAddress()!.toString();
                } else {
                    return "None";
                }
            });
            
            
            
            
            
            self.name.subscribe(self.autoSave);
            
            if (newItem) {
                self.loadFromDto(newItem, true);
            }
        }
    }
    
    export namespace Product {
    }
}
