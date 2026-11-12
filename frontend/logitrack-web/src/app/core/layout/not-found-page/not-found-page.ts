import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Icon } from '@shared/ui/icon/icon';

@Component({
  selector: 'lt-not-found-page',
  imports: [Icon, RouterLink],
  templateUrl: './not-found-page.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NotFoundPage {}
