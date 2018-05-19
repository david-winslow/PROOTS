import { Input, ViewChild } from "@angular/core";
import { FormGroup, NgForm } from "@angular/forms";
import { AlternativeServiceOptions } from "http2";
import { AlertService } from "../services/alert.service";

export abstract class Section {
    @ViewChild("form")
    protected form: NgForm;
    protected formGroup: FormGroup;
    @Input()
    public isEditMode = false;
    @Input()
    public complete = false;


    constructor(protected alertService: AlertService) {

    }


    public beginEdit() {
        this.isEditMode = true;
        return;
    }

    abstract buildForm();

    protected cancel() {
        this.buildForm();
        this.isEditMode = false;
        this.complete = false;
    }

    protected get floatLabels(): string {
        return this.isEditMode ? 'auto' : 'always';
    }

    public save(): Boolean {
        if (!this.form.submitted) {
            // Causes validation to update.
            this.form.onSubmit(null);
            return true;
        }
        if (!this.form.valid) {
            this.alertService.showValidationError();
            this.complete = false;
            return false;
                }
        else {
            this.alertService.showMessage("Saving");
            this.complete = true;
            return true;
        }

    }
}
