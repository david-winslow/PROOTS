import { Component } from "@angular/core";
import { Section } from "./Section";
import { AlertService } from "../services/alert.service";
import { FormBuilder, Validators } from "@angular/forms";

@Component({
    selector: "app-report-details",
    templateUrl: "./report-details.component.html",
    styleUrls: ["./report-details.component.scss"]
})
export class ReportDetailsComponent extends Section {

    constructor(alertService: AlertService, private formBuilder: FormBuilder) {
        super(alertService);
        this.buildForm();
    }

     buildForm() {
        this.formGroup = this.formBuilder.group({
            therapist: ['', Validators.required],
            homeLanguage: ['', Validators.required]
        });
    }
}
