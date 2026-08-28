import { Pipe, PipeTransform } from '@angular/core';

/**
 * Dates and times as a Dutch volunteer reads them.
 *
 * The zone is pinned rather than taken from the device. The server
 * decides which day is "today" in the same zone, and a phone that is
 * still on holiday time must not disagree with it about whether there is
 * an event tonight.
 */
export const APP_TIME_ZONE = 'Europe/Amsterdam';

const LOCALE = 'nl-NL';

const dayMonth = new Intl.DateTimeFormat(LOCALE, {
  weekday: 'long',
  day: 'numeric',
  month: 'long',
  timeZone: APP_TIME_ZONE,
});

const dayMonthShort = new Intl.DateTimeFormat(LOCALE, {
  weekday: 'short',
  day: 'numeric',
  month: 'short',
  timeZone: APP_TIME_ZONE,
});

const clock = new Intl.DateTimeFormat(LOCALE, {
  hour: '2-digit',
  minute: '2-digit',
  timeZone: APP_TIME_ZONE,
});

const dayKey = new Intl.DateTimeFormat('en-CA', {
  year: 'numeric',
  month: '2-digit',
  day: '2-digit',
  timeZone: APP_TIME_ZONE,
});

function capitalise(text: string): string {
  return text.charAt(0).toUpperCase() + text.slice(1);
}

/** "Zondag 6 september" */
export function formatLongDate(value: string | Date): string {
  return capitalise(dayMonth.format(new Date(value)));
}

/** "zo 6 sep" — for lists where the date is secondary. */
export function formatShortDate(value: string | Date): string {
  return dayMonthShort.format(new Date(value)).replace('.', '');
}

/** "09:30" */
export function formatTime(value: string | Date): string {
  return clock.format(new Date(value));
}

/** "09:30 – 12:00", with an en dash, as the design has it. */
export function formatTimeRange(start: string | Date, end: string | Date): string {
  return `${formatTime(start)} – ${formatTime(end)}`;
}

/** The calendar day in the app's zone, as "2026-09-06". */
export function dayOf(value: string | Date): string {
  return dayKey.format(new Date(value));
}

/** "vandaag", "morgen", or null when it is neither. */
export function relativeDayLabel(value: string | Date, now: Date = new Date()): string | null {
  const target = dayOf(value);
  if (target === dayOf(now)) return 'vandaag';
  const tomorrow = new Date(now.getTime() + 24 * 60 * 60 * 1000);
  if (target === dayOf(tomorrow)) return 'morgen';
  return null;
}

@Pipe({ name: 'longDate' })
export class LongDatePipe implements PipeTransform {
  transform(value: string | Date): string {
    return formatLongDate(value);
  }
}

@Pipe({ name: 'shortDate' })
export class ShortDatePipe implements PipeTransform {
  transform(value: string | Date): string {
    return formatShortDate(value);
  }
}

@Pipe({ name: 'clock' })
export class ClockPipe implements PipeTransform {
  transform(value: string | Date): string {
    return formatTime(value);
  }
}

@Pipe({ name: 'timeRange' })
export class TimeRangePipe implements PipeTransform {
  transform(start: string | Date, end: string | Date): string {
    return formatTimeRange(start, end);
  }
}
