
import { CreateElement } from 'vue';
import { Vue, Component, Watch, Prop } from 'vue-property-decorator';

import MetadataComponent from './c-metadata-component'
import { propDisplay } from 'coalesce-vue/lib/model'

@Component({
	name: 'c-display',
})
export default class extends MetadataComponent {
	// @Prop() public value?: any;

	@Prop({default: 'span', type: String}) 
	public element!: string;

	@Prop({default: 'L LT', type: String}) 
	public dateFormat!: string;

	render(h: CreateElement) {
        if (!this.item) return null;

		const value = propDisplay(this.item, this.propMeta);
		if (value == null) return null;
		
		return h(this.element, value)
	}
}
