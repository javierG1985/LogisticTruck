import { ChangeDetectionStrategy, Component } from '@angular/core';
import { PageHeader } from '@shared/ui/page-header/page-header';
import { ColorTokens } from '../../components/color-tokens/color-tokens';
import { ComponentGallery } from '../../components/component-gallery/component-gallery';
import { DispatchExample } from '../../components/dispatch-example/dispatch-example';
import { ShapeAndSpace } from '../../components/shape-and-space/shape-and-space';
import { TypeScale } from '../../components/type-scale/type-scale';

@Component({
  selector: 'lt-style-guide-page',
  imports: [ColorTokens, ComponentGallery, DispatchExample, PageHeader, ShapeAndSpace, TypeScale],
  templateUrl: './style-guide-page.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StyleGuidePage {}
