
import * as moment from 'moment';
import { ClassType, IHaveMetadata, ModelType } from "./metadata";


export interface Model<TMeta extends ClassType> extends IHaveMetadata {
    readonly $metadata: TMeta;
    [k: string]: any | undefined
}


export function hydrateModel<TMeta extends ClassType, TModel extends Model<TMeta>>(object: {[k: string]: any}, metadata: ClassType): TModel {
    if (!object) return object;

    const hydrated = Object.assign(object, { $metadata: metadata });
    
    for (const propName in metadata.props) {
        const propMeta = metadata.props[propName];
        const propVal = hydrated[propName];
        if (!(propName in hydrated)) {
            // All propertes that are not defined need to be declared
            // so that Vue's reactivity system can discover them.
            // Setting to undefined counts as a declaration.
            hydrated[propName] = undefined
        } else {
            switch (propMeta.type) {
                case "date":
                    // TODO: does hydrating dates into moments really belong here?    
                    if (!propVal) break;    
                    var momentInstance = moment(propVal);
                    if (!momentInstance.isValid()) {
                        throw `Recieved unparsable date ${propVal}`;
                    }
                    hydrated[propName] = momentInstance;    
                    break;    
                case "model":
                case "object":
                    hydrateModel(propVal, propMeta.typeDef)
                    break;
                case "collection":
                    const typeDef = propMeta.typeDef;
                    if (Array.isArray(propVal) 
                        && typeof(typeDef) == 'object' 
                        && (typeDef.type == "model" || typeDef.type == "object"))
                    {
                        propVal.forEach((item: any) => hydrateModel(item, typeDef));
                    }
                    break;
            }
        }
    }
    return object as TModel;
}

export function mapToDto<T extends Model<ClassType>>(object: T): any {
    var dto: { [k: string]: any } = {};
    for (const propName in object.$metadata.props) {
        const propMeta = object.$metadata.props[propName];

        var value = object[propName];
        switch (propMeta.type) {
            case "date":
                if (moment.isMoment(value)) {
                    value = value.toISOString(); // TODO: pass keepOffset property for DateTimeOffset, and not DateTime.
                } else if (value) {
                    value = value.toString();
                }
            case "string":
            case "number":
            case "boolean":
            case "enum":
                value = value || null;    
                break;
            default:
                value = undefined;
        }

        if (value !== undefined) {
            dto[propName] = value;
        }
    }
    return dto;
}




// type HydratedModel<T extends Model> = {
//     [P in keyof T]?: DeepTransport<T[P]>;
// } & IHaveMetadata


// e.g. 

/*

type DeepTransport<T> = 
    T extends any[] ? DeepTransportArray<T[number]> :
    T extends Date ? string :
    T extends object ? DeepTransportObject<T> :
    T;
interface DeepTransportArray<T> extends Array<DeepTransport<T>> {}
type DeepTransportObject<T> = { [P in keyof T]?: DeepTransport<T[P]>; };


export type Transport<T> = DeepTransport<Pick<T, Exclude<keyof T, keyof IHaveMetadata>>>



    type CaseTransport = Transport<Case>
    var a: CaseTransport;

*/