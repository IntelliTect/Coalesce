import { Event } from '../models'
import { List, DataService } from '../coalesce'


export class EventList extends List<Event> {
    public dataService = new EventService();

    constructor() {
        super()
    }
}

export class EventService extends DataService<Event> {
    public endpointBase = "/Event";

}
