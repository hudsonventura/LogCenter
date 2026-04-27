export const baseRecordLevels = [0, 1, 2, 3, 4, 5, 6] as const;

export const httpRecordLevels = [
  99,
  200,
  201,
  202,
  204,
  301,
  302,
  304,
  400,
  401,
  403,
  404,
  405,
  408,
  409,
  422,
  429,
  500,
  501,
  502,
  503,
  504,
] as const;

export const allKnownRecordLevels = [...baseRecordLevels, ...httpRecordLevels];

const baseLevelLabels: Record<number, string> = {
  0: "Trace",
  1: "Debug",
  2: "Info",
  3: "Warning",
  4: "Error",
  5: "Critical",
  6: "None",
  99: "HttpRequest",
};

const successBadgeClass =
  "bg-emerald-100 text-emerald-800 dark:bg-emerald-500/15 dark:text-emerald-300";
const redirectBadgeClass =
  "bg-cyan-100 text-cyan-800 dark:bg-cyan-500/15 dark:text-cyan-300";
const clientErrorBadgeClass =
  "bg-amber-100 text-amber-800 dark:bg-amber-500/15 dark:text-amber-300";
const serverErrorBadgeClass =
  "bg-red-600 text-white dark:bg-red-700 dark:text-red-50";

export function getLogLevelLabel(level: number): string {
  if (level in baseLevelLabels) {
    return baseLevelLabels[level];
  }

  if (isHttpResponseLevel(level)) {
    return `HTTPResponse${level}`;
  }

  return `Unknown (${level})`;
}

export function getLogLevelBadgeClass(level: number): string {
  if (level === 0) {
    return "bg-slate-100 text-slate-700 dark:bg-slate-800 dark:text-slate-100";
  }

  if (level === 1) {
    return "bg-zinc-100 text-zinc-800 dark:bg-zinc-800 dark:text-zinc-100";
  }

  if (level === 2) {
    return "bg-sky-100 text-sky-800 dark:bg-sky-500/15 dark:text-sky-300";
  }

  if (level === 3) {
    return clientErrorBadgeClass;
  }

  if (level === 4 || level === 5) {
    return serverErrorBadgeClass;
  }

  if (level === 6) {
    return "bg-stone-100 text-stone-700 dark:bg-stone-800 dark:text-stone-100";
  }

  if (level === 99) {
    return "bg-violet-100 text-violet-800 dark:bg-violet-500/15 dark:text-violet-300";
  }

  if (level >= 200 && level < 300) {
    return successBadgeClass;
  }

  if (level >= 300 && level < 400) {
    return redirectBadgeClass;
  }

  if (level >= 400 && level < 500) {
    return clientErrorBadgeClass;
  }

  if (level >= 500 && level < 600) {
    return serverErrorBadgeClass;
  }

  return "bg-slate-100 text-slate-700 dark:bg-slate-800 dark:text-slate-100";
}

export function getLogLevelChartTheme(level: number): { light: string; dark: string } {
  if (level === 0) {
    return { light: "#334155", dark: "#e2e8f0" };
  }

  if (level === 1) {
    return { light: "#3f3f46", dark: "#d4d4d8" };
  }

  if (level === 2) {
    return { light: "#075985", dark: "#38bdf8" };
  }

  if (level === 3) {
    return { light: "#b45309", dark: "#fbbf24" };
  }

  if (level === 4 || level === 5) {
    return { light: "#dc2626", dark: "#f87171" };
  }

  if (level === 6) {
    return { light: "#57534e", dark: "#d6d3d1" };
  }

  if (level === 99) {
    return { light: "#7c3aed", dark: "#c4b5fd" };
  }

  if (level >= 200 && level < 300) {
    return { light: "#15803d", dark: "#4ade80" };
  }

  if (level >= 300 && level < 400) {
    return { light: "#0f766e", dark: "#22d3ee" };
  }

  if (level >= 400 && level < 500) {
    return { light: "#b45309", dark: "#fbbf24" };
  }

  if (level >= 500 && level < 600) {
    return { light: "#b91c1c", dark: "#f87171" };
  }

  return { light: "#475569", dark: "#cbd5e1" };
}

export function isHttpResponseLevel(level: number): boolean {
  return level >= 200 && level < 600;
}
