// ID are max 5 digit hex. This is shared between anything that needs an ID.
export const MIN_ID = 0;
export const MAX_ID = 1048575;

export const FILE_EXTENSION = ".taskp";

export const SECTIONS = {
    NONE: 'NONE',
    TASKS: 'TASKS',
    ISSUES: 'ISSUES',
    USERS: 'USERS',
    SETTINGS: 'SETTINGS',
};

export const SETTINGS_KEYS = {
    SCANNER_KEYWORD: 'Scanner.Keyword',
    SCANNER_EXCLUDE: 'Scanner.Exclude',
};