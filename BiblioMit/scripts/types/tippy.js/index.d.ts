declare class tippy {
    constructor(selector: string, options: TippyOptions)
}

declare interface TippyOptions {
    content?: string;
    allowHTML?: boolean;
    animation?: string;
}