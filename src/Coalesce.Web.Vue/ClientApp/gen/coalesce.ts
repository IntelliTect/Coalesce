

export abstract class DataService<T> {
    public abstract endpointBase: string;
}

export abstract class List<T> {
    public abstract dataService: DataService<T>;

    private _pageSize = 10;
    get pageSize() {
        return this._pageSize;
    }
    set pageSize(pageSize: number) {
        this._pageSize = pageSize;
    }

    private _page = 1;
    public get page() {
        return this._page;
    }
    public set page(page: number) {
        this._page = page;
    }

    public nextPage() {
        this.page++;
        this.load();
    }

    public items: T[] = [];

    public load() {
        fetch(`${this.dataService.endpointBase}/List?page=${this.page}&pageSize=${this.pageSize}`)
            .then(response => response.json() as Promise<T[]>)
            .then(items => this.items = items);

    }
}