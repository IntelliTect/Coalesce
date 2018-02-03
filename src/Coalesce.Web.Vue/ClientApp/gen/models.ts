

export interface Event {
    eventId: number;
    title: string;
    notes?: EventNote[];
    eventApplications?: EventApplication[];
}

export interface EventNote {
    eventNoteId: number;
    event?: Event;
    eventId: number;
    text: string;
}

export interface Application {
    applicationId: number;
    name: string;
}

export interface EventApplication {
    eventApplicationId: number;
    eventId: number;
    event?: Event;
    applicationId: number;
    application?: Application;
}