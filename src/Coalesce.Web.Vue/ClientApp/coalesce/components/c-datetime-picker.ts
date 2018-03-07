
// Tedious imports for maximum tree shaking
import * as isValid from 'date-fns/isValid'
import * as toDate from 'date-fns/toDate'
import * as format from 'date-fns/format'
import * as parse from 'date-fns/parse'
import * as setYear from 'date-fns/setYear'
import * as setMonth from 'date-fns/setMonth'
import * as setDate from 'date-fns/setDate'
import * as setHours from 'date-fns/setHours'
import * as setMinutes from 'date-fns/setMinutes'

// import { format, parse, isValid, setYear, setMonth, setDate, setHours, setMinutes } from 'date-fns'
import { Vue, Component, Watch, Prop } from 'vue-property-decorator';
// import CDisplay from './c-display';
import MetadataComponent from './c-metadata-component'
import { ModelProperty } from 'coalesce-vue/lib/metadata';

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

    @Prop({default: 'MM/DD/YYYY h:mm A', type: String}) 
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
                value = toDate(val)
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

    get datePart() { return this.value && format(this.value, "YYYY-MM-DD") || null }
    get timePart() { return this.value && format(this.value, "HH:mm") || null }
}

