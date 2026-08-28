import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { ApiClient } from '../../core/api-client';
import { CachedResource } from '../../core/cached-resource';
import type { ContactCardEntry, GuidelineCard } from '../../core/api.types';
import { UiButton } from '../../ui/button';
import { UiCard } from '../../ui/card';
import { UiIcon } from '../../ui/icon';

/**
 * The contact card and the generally visible guideline cards.
 *
 * Both are cached, because the place where you need a phone number is
 * usually the place with no signal. Restricted guidelines are not here
 * and not cached — the server does not send them at all, so there is no
 * padlock to be tempted by.
 *
 * The guideline HTML is sanitized server-side with an allow-list and
 * rendered through Angular's own sanitizer as well. Never
 * bypassSecurityTrustHtml.
 */
@Component({
  selector: 'app-info-page',
  imports: [UiButton, UiCard, UiIcon],
  template: `
    @let contacts = contactsResource.value();
    @let guidelines = guidelinesResource.value();

    <div class="stack">
      <h2>Informatie</h2>

      <ui-card title="Contactkaart" meta="Offline beschikbaar">
        @if (!contacts) {
          <p class="muted small">Contacten worden opgehaald.</p>
        } @else if (contacts.length === 0) {
          <p class="muted small">Er staan nog geen contacten in.</p>
        } @else {
          <ul class="contacts">
            @for (contact of contacts; track contact.id) {
              <li>
                <span class="who">
                  <span class="name">{{ contact.name }}</span>
                  @if (contact.function) {
                    <span class="muted small">{{ contact.function }}</span>
                  }
                </span>
                <a
                  uiButton
                  [variant]="contact.isEmergencyNumber ? 'danger' : 'secondary'"
                  size="md"
                  [href]="telHref(contact.phone)"
                >
                  <ui-icon name="phone" />
                  {{ contact.phone }}
                </a>
              </li>
            }
          </ul>
        }
      </ui-card>

      @for (guideline of guidelines ?? []; track guideline.id) {
        <ui-card [title]="guideline.title" [meta]="'Versie ' + guideline.version">
          <div class="prose" [innerHTML]="guideline.sanitizedHtml"></div>
        </ui-card>
      }
    </div>
  `,
  styles: [
    `
      .contacts {
        list-style: none;
        margin: 0;
        padding: 0;
        display: flex;
        flex-direction: column;
        gap: var(--space-2);
      }

      .contacts > li {
        display: flex;
        align-items: center;
        gap: var(--space-3);
        min-height: var(--hit-min);
      }

      .who {
        flex: 1;
        min-width: 0;
        display: flex;
        flex-direction: column;
        gap: 2px;
      }

      .name {
        font-size: var(--text-body-md);
        color: var(--text-heading);
      }

      .prose {
        font-size: var(--text-body-sm);
        line-height: var(--leading-relaxed);
        color: var(--text-body);
      }
    `,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class InfoPage {
  private readonly api = inject(ApiClient);

  readonly contactsResource = new CachedResource<ContactCardEntry[]>(
    this.api,
    '/api/info/contacts',
  );
  readonly guidelinesResource = new CachedResource<GuidelineCard[]>(
    this.api,
    '/api/info/guidelines',
  );

  constructor() {
    void this.contactsResource.reload();
    void this.guidelinesResource.reload();
  }

  telHref(phone: string): string {
    return `tel:${phone.replace(/\s/g, '')}`;
  }
}
