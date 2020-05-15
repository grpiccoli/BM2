declare class Choices {
    ajax(callback: any): any;
    constructor(element: HTMLElement, options: ChoicesOptions);
    hideDropdown(): void;
    setChoices(callback: any): void;
    getValue(val?: boolean): any;
    disable(): void;
    removeActiveItemsByValue(value: any): void;
    removeActiveItems(): void;
    setChoiceByValue(value: any): void;
    enable(): void;
}

declare interface ChoicesOptions {
    maxItemCount?: number;
    removeItemButton?: boolean;
    duplicateItemsAllowed?: boolean;
    paste?: boolean;
    searchFields?: string[];
    shouldSort?: boolean;
    placeholderValue?: string;
    searchPlaceholderValue?: string;
    searchResultLimit?: number;
    loadingText?: string;
    noResultsText?: string;
    noChoicesText?: string;
    itemSelectText?: string;
    maxItemText?: any;
    fuseOptions?: any;
}
