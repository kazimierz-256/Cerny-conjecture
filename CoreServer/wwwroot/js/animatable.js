class Animatable {
    constructor(updateAction, initAction, destroyAction, subject) {
        this.updateAction = updateAction;
        this.initAction = initAction;
        this.destroyAction = destroyAction;
        this.subject = subject;
    }
    init(scene, appSettings) {
        if (this.initAction != undefined)
            this.initAction(scene, appSettings);
    }
    update(t, appSettings) {
        if (this.updateAction != undefined)
            this.updateAction(t, appSettings);
    }
    destroy(t, appSettings) {
        if (this.destroyAction != undefined)
            this.destroyAction(t, appSettings);
    }
    subject() {
        if (this.subject != undefined)
            this.subject();
    }
}