export declare type OwnProps<T, TExclude> = Pick<T, Exclude<keyof T, keyof TExclude>>;
export declare type Indexable<T> = T & {
    [k: string]: any | undefined;
};
