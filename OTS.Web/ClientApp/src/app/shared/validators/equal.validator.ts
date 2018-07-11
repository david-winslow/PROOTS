



import { AbstractControl, ValidatorFn } from '@angular/forms'

export function EqualValidator(controlName: string): ValidatorFn {
    return (control: AbstractControl): { [key: string]: any } => {
        const compareControl = control.parent ? control.parent.get(controlName) : null;
        const areEqual = compareControl && control.value === compareControl.value;
        return areEqual ? null : { notEqual: true };
    };
}
