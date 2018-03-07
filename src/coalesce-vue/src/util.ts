
export type OwnProps<T, TExclude> = Pick<T, Exclude<keyof T, keyof TExclude>>

export type Indexable<T> = T & { [k: string]: any | undefined }