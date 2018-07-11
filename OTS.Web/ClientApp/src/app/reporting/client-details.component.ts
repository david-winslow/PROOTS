import { Component, OnChanges, SimpleChanges } from "@angular/core";
import { Section } from "./Section";
import { AlertService } from "../services/alert.service";
import { FormBuilder, Validators } from "@angular/forms";

@Component({
    selector: "app-client-details",
    templateUrl: "./client-details.component.html",
    styleUrls: ["./client-details.component.scss"]
})
export class ClientDetailsComponent extends Section implements OnChanges {

    ngOnChanges(changes: SimpleChanges): void {
        this.alertService.resetStickyMessage();
    }
    constructor(alertService: AlertService, private formBuilder: FormBuilder) {
        super(alertService);
        this.buildForm();
    }

     buildForm() {
        this.formGroup = this.formBuilder.group({
            clientName: ['', Validators.required],
            homeLanguage: ['', Validators.required]
        });
    }
}
