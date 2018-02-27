
export type OwnProps<T, TExclude> = Pick<T, Exclude<keyof T, keyof TExclude>>