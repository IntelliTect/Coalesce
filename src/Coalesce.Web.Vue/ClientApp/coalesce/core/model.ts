import { ClassType, IHaveMetadata } from "./metadata";
import { Property } from ".";

export interface Model<TMeta extends ClassType> extends IHaveMetadata {
    readonly $metadata: TMeta;
}

// type HydratedModel<T extends Model> = {
//     [P in keyof T]?: DeepTransport<T[P]>;
// } & IHaveMetadata


type DeepTransport<T> = 
    T extends any[] ? DeepTransportArray<T[number]> :
    T extends Date ? string :
    T extends object ? DeepTransportObject<T> :
    T;
interface DeepTransportArray<T> extends Array<DeepTransport<T>> {}
type DeepTransportObject<T> = { [P in keyof T]?: DeepTransport<T[P]>; };


export type Transport<T> = DeepTransport<Pick<T, Exclude<keyof T, keyof IHaveMetadata>>>



// e.g. 

/*
    type CaseTransport = Transport<Case>
    var a: CaseTransport;

*/