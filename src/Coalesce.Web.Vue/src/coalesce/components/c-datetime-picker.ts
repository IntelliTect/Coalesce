
// This will tree shake correctly as of v2.0.0-alpha.21
import { isValid, toDate, format, parse, setYear, setMonth, setDate, setHours, setMinutes } from 'date-fns';

import { Vue, Component, Watch, Prop } from 'vue-property-decorator';

@Component({
    name: 'c-datetime-picker',
    components: {
        // CDisplay
    }
})
export default class extends Vue {

    @Prop({required: false, type: Date})
    public value?: Date | null ;

    @Prop({type: String})
    public label?: string;

    @Prop({default: 'MM/dd/yyyy h:mm aa', type: String}) 
    public dateFormat!: string;

    get displayedValue() {
        return this.value && format(this.value, this.dateFormat) || ''
    }

    public error: string[] = []
    
    textInputChanged(val: string) {
        this.error = []
        var value: Date | null | undefined;
        if (!val || !val.trim()) {
            value = null
        } else {
            value = parse(val, this.dateFormat, new Date())
            
            // If the input didn't match our format exactly, 
            // try parsing user input with general formatting interpretation (trying to be a good citizen)
            if (!isValid(value)) {
                value = new Date(val);
            }
            
            // If that didn't work, don't change the underlying value. Instead, display an error.
            if (!value || !isValid(value)) {
                
                // TODO: i18n
                this.error = ["Invalid Date. Try formatting like " + format(new Date(), this.dateFormat)]
                value = null
            }
        }

        // Only emit an event if the input isn't invalid.
        // If we don't emit an input event, it gives the user a chance to correct their text.
        if (!this.error.length) this.$emit('input', value)
    }

    timeChanged(val: string) {
        var value = this.value || new Date()

        var parts = /(\d\d):(\d\d)/.exec(val);
        if (!parts) throw `Time set by vuetify timepicker not in expected format: ${val}`

        value = setHours(value, parseInt(parts[1]))
        value = setMinutes(value, parseInt(parts[2]))
        this.$emit('input', value)
    }

    dateChanged(val: string) {
        var value = this.value || new Date()

        var parts = /(\d\d\d\d)-(\d\d)-(\d\d)/.exec(val);
        if (!parts) throw `Date set by vuetify datepicker not in expected format: ${val}`

        value = setYear(value, parseInt(parts[1]))
        value = setMonth(value, parseInt(parts[2]) - 1)
        value = setDate(value, parseInt(parts[3]))
        this.$emit('input', value)
    }

    menu = false
    selectedTab: "date" | "time" | null = null; // = "date"

    get datePart() { return this.value && format(this.value, "yyyy-MM-dd") || null }
    get timePart() { return this.value && format(this.value, "HH:mm") || null }
}

