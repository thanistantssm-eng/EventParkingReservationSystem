import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import * as i0 from "@angular/core";
import * as i1 from "@angular/forms";
function Login_Conditional_14_Template(rf, ctx) { if (rf & 1) {
    const _r1 = i0.ɵɵgetCurrentView();
    i0.ɵɵelementStart(0, "div", 11)(1, "button", 52);
    i0.ɵɵlistener("click", function Login_Conditional_14_Template_button_click_1_listener() { i0.ɵɵrestoreView(_r1); const ctx_r1 = i0.ɵɵnextContext(); return i0.ɵɵresetView(ctx_r1.languageOpen.set(false)); });
    i0.ɵɵtext(2, "English");
    i0.ɵɵelementEnd()();
} }
function Login_Conditional_47_Template(rf, ctx) { if (rf & 1) {
    i0.ɵɵtext(0, " \u2713 ");
} }
function Login_Conditional_57_Template(rf, ctx) { if (rf & 1) {
    i0.ɵɵelementStart(0, "p", 40);
    i0.ɵɵtext(1);
    i0.ɵɵelementEnd();
} if (rf & 2) {
    const ctx_r1 = i0.ɵɵnextContext();
    i0.ɵɵadvance();
    i0.ɵɵtextInterpolate(ctx_r1.message());
} }
export class Login {
    passwordVisible = signal(false, /* @ts-ignore */
    ...(ngDevMode ? [{ debugName: "passwordVisible" }] : /* istanbul ignore next */ []));
    rememberMe = signal(true, /* @ts-ignore */
    ...(ngDevMode ? [{ debugName: "rememberMe" }] : /* istanbul ignore next */ []));
    languageOpen = signal(false, /* @ts-ignore */
    ...(ngDevMode ? [{ debugName: "languageOpen" }] : /* istanbul ignore next */ []));
    message = signal('', /* @ts-ignore */
    ...(ngDevMode ? [{ debugName: "message" }] : /* istanbul ignore next */ []));
    emailOrUsername = '';
    password = '';
    togglePassword() {
        this.passwordVisible.update((value) => !value);
    }
    toggleRemember() {
        this.rememberMe.update((value) => !value);
    }
    submit() {
        this.message.set('');
        if (!this.emailOrUsername.trim() || !this.password.trim()) {
            this.message.set('Enter your email or username and password.');
            return;
        }
        this.message.set('Login form is ready for backend integration.');
    }
    static ɵfac = function Login_Factory(__ngFactoryType__) { return new (__ngFactoryType__ || Login)(); };
    static ɵcmp = /*@__PURE__*/ i0.ɵɵdefineComponent({ type: Login, selectors: [["app-login"]], decls: 82, vars: 11, consts: [[1, "login-page"], ["aria-label", "Event Parking Reservation System", 1, "visual-panel"], [1, "sr-only"], [1, "form-panel"], [1, "language-wrap"], ["type", "button", "aria-haspopup", "menu", 1, "language-button", 3, "click"], ["viewBox", "0 0 24 24", "aria-hidden", "true"], ["cx", "12", "cy", "12", "r", "9"], ["d", "M3 12h18M12 3a15 15 0 0 1 0 18M12 3a15 15 0 0 0 0 18"], ["viewBox", "0 0 24 24", "aria-hidden", "true", 1, "chevron"], ["d", "m8 10 4 4 4-4"], ["role", "menu", 1, "language-menu"], ["aria-labelledby", "login-title", 1, "login-card"], ["aria-hidden", "true", 1, "lock-badge"], ["viewBox", "0 0 24 24"], ["x", "6", "y", "10", "width", "12", "height", "10", "rx", "2"], ["d", "M9 10V7a3 3 0 0 1 6 0v3"], ["id", "login-title"], [1, "subtitle"], ["novalidate", "", 3, "ngSubmit"], ["for", "login-id", 1, "field-label"], [1, "input-shell"], ["viewBox", "0 0 24 24", "aria-hidden", "true", 1, "field-icon"], ["x", "3", "y", "5", "width", "18", "height", "14", "rx", "2"], ["d", "m4 7 8 6 8-6"], ["id", "login-id", "name", "emailOrUsername", "type", "text", "autocomplete", "username", "placeholder", "Enter your email or username", 3, "ngModelChange", "ngModel"], ["for", "login-password", 1, "field-label", "password-label"], ["x", "5", "y", "10", "width", "14", "height", "10", "rx", "2"], ["d", "M8 10V7a4 4 0 0 1 8 0v3"], ["id", "login-password", "name", "password", "autocomplete", "current-password", "placeholder", "Enter your password", 3, "ngModelChange", "type", "ngModel"], ["type", "button", 1, "eye-button", 3, "click"], ["d", "M2.5 12s3.5-6 9.5-6 9.5 6 9.5 6-3.5 6-9.5 6-9.5-6-9.5-6Z"], ["cx", "12", "cy", "12", "r", "2.5"], [1, "form-options"], [1, "remember-control"], ["type", "checkbox", 1, "sr-only", 3, "change", "checked"], [1, "check-box"], ["href", "#", 1, "forgot-link"], ["type", "submit", 1, "sign-in-button"], ["d", "M5 12h14M13 6l6 6-6 6"], ["role", "status", 1, "form-message"], ["aria-hidden", "true", 1, "divider-row"], ["aria-label", "Social sign in options", 1, "social-buttons"], ["type", "button", "aria-label", "Continue with Google", 1, "social-button"], [1, "google-g"], ["type", "button", "aria-label", "Continue with Apple", 1, "social-button", "apple-button"], ["d", "M16.7 12.7c0-2.5 2-3.7 2.1-3.8-1.2-1.7-3-1.9-3.7-1.9-1.6-.2-3.1.9-3.9.9-.8 0-2-.9-3.4-.9-1.7 0-3.4 1-4.3 2.6-1.9 3.2-.5 8 1.3 10.6.9 1.3 2 2.8 3.5 2.7 1.4-.1 1.9-.9 3.7-.9 1.7 0 2.2.9 3.7.9 1.6 0 2.5-1.3 3.4-2.6 1-1.5 1.5-3 1.5-3.1-.1 0-2.9-1.1-2.9-4.5Zm-2.6-7.4c.8-.9 1.3-2.2 1.2-3.3-1.1.1-2.4.7-3.2 1.6-.7.8-1.3 2.1-1.2 3.2 1.2.1 2.4-.6 3.2-1.5Z"], ["type", "button", "aria-label", "Continue with Microsoft", 1, "social-button"], ["aria-hidden", "true", 1, "microsoft-logo"], [1, "create-row"], ["href", "#"], ["routerLink", "/", 1, "back-home"], ["type", "button", "role", "menuitem", 3, "click"]], template: function Login_Template(rf, ctx) { if (rf & 1) {
            i0.ɵɵelementStart(0, "main", 0)(1, "section", 1)(2, "span", 2);
            i0.ɵɵtext(3, " Event Parking Reservation System. Book events, reserve parking, and manage your tickets seamlessly. ");
            i0.ɵɵelementEnd()();
            i0.ɵɵelementStart(4, "section", 3)(5, "div", 4)(6, "button", 5);
            i0.ɵɵlistener("click", function Login_Template_button_click_6_listener() { return ctx.languageOpen.set(!ctx.languageOpen()); });
            i0.ɵɵnamespaceSVG();
            i0.ɵɵelementStart(7, "svg", 6);
            i0.ɵɵelement(8, "circle", 7)(9, "path", 8);
            i0.ɵɵelementEnd();
            i0.ɵɵnamespaceHTML();
            i0.ɵɵelementStart(10, "span");
            i0.ɵɵtext(11, "English");
            i0.ɵɵelementEnd();
            i0.ɵɵnamespaceSVG();
            i0.ɵɵelementStart(12, "svg", 9);
            i0.ɵɵelement(13, "path", 10);
            i0.ɵɵelementEnd()();
            i0.ɵɵconditionalCreate(14, Login_Conditional_14_Template, 3, 0, "div", 11);
            i0.ɵɵelementEnd();
            i0.ɵɵnamespaceHTML();
            i0.ɵɵelementStart(15, "section", 12)(16, "div", 13);
            i0.ɵɵnamespaceSVG();
            i0.ɵɵelementStart(17, "svg", 14);
            i0.ɵɵelement(18, "rect", 15)(19, "path", 16);
            i0.ɵɵelementEnd()();
            i0.ɵɵnamespaceHTML();
            i0.ɵɵelementStart(20, "h1", 17);
            i0.ɵɵtext(21, "Welcome Back");
            i0.ɵɵelementEnd();
            i0.ɵɵelementStart(22, "p", 18);
            i0.ɵɵtext(23, "Sign in to continue to your account");
            i0.ɵɵelementEnd();
            i0.ɵɵelementStart(24, "form", 19);
            i0.ɵɵlistener("ngSubmit", function Login_Template_form_ngSubmit_24_listener() { return ctx.submit(); });
            i0.ɵɵelementStart(25, "label", 20);
            i0.ɵɵtext(26, "Email or Username");
            i0.ɵɵelementEnd();
            i0.ɵɵelementStart(27, "div", 21);
            i0.ɵɵnamespaceSVG();
            i0.ɵɵelementStart(28, "svg", 22);
            i0.ɵɵelement(29, "rect", 23)(30, "path", 24);
            i0.ɵɵelementEnd();
            i0.ɵɵnamespaceHTML();
            i0.ɵɵelementStart(31, "input", 25);
            i0.ɵɵtwoWayListener("ngModelChange", function Login_Template_input_ngModelChange_31_listener($event) { i0.ɵɵtwoWayBindingSet(ctx.emailOrUsername, $event) || (ctx.emailOrUsername = $event); return $event; });
            i0.ɵɵelementEnd();
            i0.ɵɵcontrolCreate();
            i0.ɵɵelementEnd();
            i0.ɵɵelementStart(32, "label", 26);
            i0.ɵɵtext(33, "Password");
            i0.ɵɵelementEnd();
            i0.ɵɵelementStart(34, "div", 21);
            i0.ɵɵnamespaceSVG();
            i0.ɵɵelementStart(35, "svg", 22);
            i0.ɵɵelement(36, "rect", 27)(37, "path", 28);
            i0.ɵɵelementEnd();
            i0.ɵɵnamespaceHTML();
            i0.ɵɵelementStart(38, "input", 29);
            i0.ɵɵtwoWayListener("ngModelChange", function Login_Template_input_ngModelChange_38_listener($event) { i0.ɵɵtwoWayBindingSet(ctx.password, $event) || (ctx.password = $event); return $event; });
            i0.ɵɵelementEnd();
            i0.ɵɵcontrolCreate();
            i0.ɵɵelementStart(39, "button", 30);
            i0.ɵɵlistener("click", function Login_Template_button_click_39_listener() { return ctx.togglePassword(); });
            i0.ɵɵnamespaceSVG();
            i0.ɵɵelementStart(40, "svg", 6);
            i0.ɵɵelement(41, "path", 31)(42, "circle", 32);
            i0.ɵɵelementEnd()()();
            i0.ɵɵnamespaceHTML();
            i0.ɵɵelementStart(43, "div", 33)(44, "label", 34)(45, "input", 35);
            i0.ɵɵlistener("change", function Login_Template_input_change_45_listener() { return ctx.toggleRemember(); });
            i0.ɵɵelementEnd();
            i0.ɵɵelementStart(46, "span", 36);
            i0.ɵɵconditionalCreate(47, Login_Conditional_47_Template, 1, 0);
            i0.ɵɵelementEnd();
            i0.ɵɵelementStart(48, "span");
            i0.ɵɵtext(49, "Remember me");
            i0.ɵɵelementEnd()();
            i0.ɵɵelementStart(50, "a", 37);
            i0.ɵɵtext(51, "Forgot Password?");
            i0.ɵɵelementEnd()();
            i0.ɵɵelementStart(52, "button", 38)(53, "span");
            i0.ɵɵtext(54, "Sign In");
            i0.ɵɵelementEnd();
            i0.ɵɵnamespaceSVG();
            i0.ɵɵelementStart(55, "svg", 6);
            i0.ɵɵelement(56, "path", 39);
            i0.ɵɵelementEnd()();
            i0.ɵɵconditionalCreate(57, Login_Conditional_57_Template, 2, 1, "p", 40);
            i0.ɵɵelementEnd();
            i0.ɵɵnamespaceHTML();
            i0.ɵɵelementStart(58, "div", 41);
            i0.ɵɵelement(59, "span");
            i0.ɵɵelementStart(60, "p");
            i0.ɵɵtext(61, "or continue with");
            i0.ɵɵelementEnd();
            i0.ɵɵelement(62, "span");
            i0.ɵɵelementEnd();
            i0.ɵɵelementStart(63, "div", 42)(64, "button", 43)(65, "span", 44);
            i0.ɵɵtext(66, "G");
            i0.ɵɵelementEnd()();
            i0.ɵɵelementStart(67, "button", 45);
            i0.ɵɵnamespaceSVG();
            i0.ɵɵelementStart(68, "svg", 6);
            i0.ɵɵelement(69, "path", 46);
            i0.ɵɵelementEnd()();
            i0.ɵɵnamespaceHTML();
            i0.ɵɵelementStart(70, "button", 47)(71, "span", 48);
            i0.ɵɵelement(72, "i")(73, "i")(74, "i")(75, "i");
            i0.ɵɵelementEnd()()();
            i0.ɵɵelementStart(76, "p", 49);
            i0.ɵɵtext(77, " Don\u2019t have an account? ");
            i0.ɵɵelementStart(78, "a", 50);
            i0.ɵɵtext(79, "Create Account");
            i0.ɵɵelementEnd()();
            i0.ɵɵelementStart(80, "a", 51);
            i0.ɵɵtext(81, "\u2190 Back to Eventora");
            i0.ɵɵelementEnd()()()();
        } if (rf & 2) {
            i0.ɵɵadvance(6);
            i0.ɵɵattribute("aria-expanded", ctx.languageOpen());
            i0.ɵɵadvance(8);
            i0.ɵɵconditional(ctx.languageOpen() ? 14 : -1);
            i0.ɵɵadvance(17);
            i0.ɵɵtwoWayProperty("ngModel", ctx.emailOrUsername);
            i0.ɵɵcontrol();
            i0.ɵɵadvance(7);
            i0.ɵɵproperty("type", ctx.passwordVisible() ? "text" : "password");
            i0.ɵɵtwoWayProperty("ngModel", ctx.password);
            i0.ɵɵcontrol();
            i0.ɵɵadvance();
            i0.ɵɵattribute("aria-label", ctx.passwordVisible() ? "Hide password" : "Show password");
            i0.ɵɵadvance(6);
            i0.ɵɵproperty("checked", ctx.rememberMe());
            i0.ɵɵadvance();
            i0.ɵɵclassProp("checked", ctx.rememberMe());
            i0.ɵɵadvance();
            i0.ɵɵconditional(ctx.rememberMe() ? 47 : -1);
            i0.ɵɵadvance(10);
            i0.ɵɵconditional(ctx.message() ? 57 : -1);
        } }, dependencies: [FormsModule, i1.ɵNgNoValidate, i1.DefaultValueAccessor, i1.NgControlStatus, i1.NgControlStatusGroup, i1.NgModel, i1.NgForm, RouterLink], styles: ["[_nghost-%COMP%] {\n  display: block;\n  min-height: 100vh;\n}\n\n*[_ngcontent-%COMP%] {\n  box-sizing: border-box;\n}\n\n.login-page[_ngcontent-%COMP%] {\n  min-height: 100vh;\n  display: grid;\n  grid-template-columns: 54.7% 45.3%;\n  background: #ffffff;\n  color: #10162f;\n  font-family: Inter, Arial, sans-serif;\n}\n\n.visual-panel[_ngcontent-%COMP%] {\n  position: relative;\n  min-height: 100vh;\n  background: #130b31 url('/assets/login-left-visual.jpg') center / 100% 100% no-repeat;\n  overflow: hidden;\n}\n\n.form-panel[_ngcontent-%COMP%] {\n  position: relative;\n  min-height: 100vh;\n  padding: 30px 62px 42px 66px;\n  background:\n    radial-gradient(circle at 77% 72%, rgba(255, 116, 24, .025), transparent 34%),\n    #ffffff;\n}\n\n.language-wrap[_ngcontent-%COMP%] {\n  position: relative;\n  display: flex;\n  justify-content: flex-end;\n  min-height: 61px;\n}\n\n.language-button[_ngcontent-%COMP%] {\n  width: 145px;\n  height: 43px;\n  padding: 0 14px 0 16px;\n  display: grid;\n  grid-template-columns: 20px 1fr 16px;\n  align-items: center;\n  gap: 9px;\n  border: 1px solid #dde1e8;\n  border-radius: 14px;\n  color: #131a31;\n  background: #ffffff;\n  font-size: 14px;\n  font-weight: 650;\n  box-shadow: 0 3px 10px rgba(42, 52, 85, .03);\n}\n\n.language-button[_ngcontent-%COMP%]   svg[_ngcontent-%COMP%] {\n  width: 19px;\n  height: 19px;\n  fill: none;\n  stroke: currentColor;\n  stroke-width: 1.8;\n  stroke-linecap: round;\n  stroke-linejoin: round;\n}\n\n.language-button[_ngcontent-%COMP%]   .chevron[_ngcontent-%COMP%] {\n  width: 15px;\n  height: 15px;\n  justify-self: end;\n}\n\n.language-menu[_ngcontent-%COMP%] {\n  position: absolute;\n  z-index: 5;\n  top: 48px;\n  right: 0;\n  width: 145px;\n  padding: 6px;\n  border: 1px solid #e4e7ed;\n  border-radius: 12px;\n  background: #ffffff;\n  box-shadow: 0 14px 35px rgba(31, 39, 74, .12);\n}\n\n.language-menu[_ngcontent-%COMP%]   button[_ngcontent-%COMP%] {\n  width: 100%;\n  padding: 9px 11px;\n  border: 0;\n  border-radius: 8px;\n  text-align: left;\n  background: transparent;\n}\n\n.language-menu[_ngcontent-%COMP%]   button[_ngcontent-%COMP%]:hover {\n  background: #fff4ec;\n}\n\n.login-card[_ngcontent-%COMP%] {\n  width: 100%;\n  min-height: 768px;\n  margin-top: 0;\n  padding: 47px 48px 38px;\n  border: 1px solid #f0e5df;\n  border-radius: 24px;\n  background: rgba(255, 255, 255, .98);\n  box-shadow: 0 17px 40px rgba(33, 41, 75, .055);\n}\n\n.lock-badge[_ngcontent-%COMP%] {\n  width: 54px;\n  height: 54px;\n  margin-bottom: 22px;\n  display: grid;\n  place-items: center;\n  border-radius: 50%;\n  color: #ff6508;\n  background: #fff3ec;\n}\n\n.lock-badge[_ngcontent-%COMP%]   svg[_ngcontent-%COMP%] {\n  width: 24px;\n  height: 24px;\n  fill: none;\n  stroke: currentColor;\n  stroke-width: 2;\n  stroke-linecap: round;\n  stroke-linejoin: round;\n}\n\nh1[_ngcontent-%COMP%] {\n  margin: 0;\n  color: #10162f;\n  font-size: 36px;\n  line-height: 1.08;\n  font-weight: 800;\n  letter-spacing: -1.15px;\n}\n\n.subtitle[_ngcontent-%COMP%] {\n  margin: 11px 0 34px;\n  color: #737b94;\n  font-size: 15px;\n  line-height: 1.45;\n}\n\n.field-label[_ngcontent-%COMP%] {\n  display: block;\n  margin-bottom: 10px;\n  color: #12182d;\n  font-size: 13px;\n  font-weight: 700;\n}\n\n.password-label[_ngcontent-%COMP%] {\n  margin-top: 23px;\n}\n\n.input-shell[_ngcontent-%COMP%] {\n  height: 53px;\n  display: flex;\n  align-items: center;\n  gap: 13px;\n  padding: 0 16px;\n  border: 1px solid #d9dee8;\n  border-radius: 10px;\n  background: #ffffff;\n  transition: border-color .2s ease, box-shadow .2s ease;\n}\n\n.input-shell[_ngcontent-%COMP%]:focus-within {\n  border-color: #ff8a45;\n  box-shadow: 0 0 0 3px rgba(255, 105, 20, .09);\n}\n\n.field-icon[_ngcontent-%COMP%] {\n  flex: 0 0 21px;\n  width: 21px;\n  height: 21px;\n  fill: none;\n  stroke: #7a849a;\n  stroke-width: 1.8;\n  stroke-linecap: round;\n  stroke-linejoin: round;\n}\n\n.input-shell[_ngcontent-%COMP%]   input[_ngcontent-%COMP%] {\n  min-width: 0;\n  width: 100%;\n  height: 100%;\n  padding: 0;\n  border: 0;\n  outline: 0;\n  color: #182039;\n  background: transparent;\n  font-size: 14px;\n}\n\n.input-shell[_ngcontent-%COMP%]   input[_ngcontent-%COMP%]::placeholder {\n  color: #8c94a9;\n}\n\n.eye-button[_ngcontent-%COMP%] {\n  flex: 0 0 32px;\n  width: 32px;\n  height: 32px;\n  display: grid;\n  place-items: center;\n  border: 0;\n  border-radius: 8px;\n  color: #737d93;\n  background: transparent;\n}\n\n.eye-button[_ngcontent-%COMP%]:hover {\n  background: #f7f8fa;\n}\n\n.eye-button[_ngcontent-%COMP%]   svg[_ngcontent-%COMP%] {\n  width: 21px;\n  height: 21px;\n  fill: none;\n  stroke: currentColor;\n  stroke-width: 1.8;\n  stroke-linecap: round;\n  stroke-linejoin: round;\n}\n\n.form-options[_ngcontent-%COMP%] {\n  margin: 22px 0 25px;\n  display: flex;\n  align-items: center;\n  justify-content: space-between;\n  gap: 20px;\n}\n\n.remember-control[_ngcontent-%COMP%] {\n  display: inline-flex;\n  align-items: center;\n  gap: 10px;\n  color: #31384f;\n  font-size: 14px;\n  cursor: pointer;\n  user-select: none;\n}\n\n.check-box[_ngcontent-%COMP%] {\n  width: 19px;\n  height: 19px;\n  display: grid;\n  place-items: center;\n  border: 1px solid #d4d9e2;\n  border-radius: 4px;\n  color: #ffffff;\n  background: #ffffff;\n  font-size: 12px;\n  font-weight: 800;\n}\n\n.check-box.checked[_ngcontent-%COMP%] {\n  border-color: #ff6908;\n  background: #ff6908;\n}\n\n.forgot-link[_ngcontent-%COMP%] {\n  color: #ff5b00;\n  font-size: 14px;\n  text-decoration: none;\n}\n\n.forgot-link[_ngcontent-%COMP%]:hover, \n.create-row[_ngcontent-%COMP%]   a[_ngcontent-%COMP%]:hover {\n  text-decoration: underline;\n}\n\n.sign-in-button[_ngcontent-%COMP%] {\n  width: 100%;\n  height: 54px;\n  padding: 0 32px;\n  display: grid;\n  grid-template-columns: 1fr auto 1fr;\n  align-items: center;\n  border: 0;\n  border-radius: 10px;\n  color: #ffffff;\n  background: linear-gradient(90deg, #ff7a00 0%, #ff5600 100%);\n  box-shadow: 0 8px 18px rgba(255, 95, 0, .13);\n  font-size: 16px;\n  font-weight: 750;\n  transition: transform .2s ease, box-shadow .2s ease;\n}\n\n.sign-in-button[_ngcontent-%COMP%]   span[_ngcontent-%COMP%] {\n  grid-column: 2;\n}\n\n.sign-in-button[_ngcontent-%COMP%]   svg[_ngcontent-%COMP%] {\n  grid-column: 3;\n  justify-self: end;\n  width: 24px;\n  height: 24px;\n  fill: none;\n  stroke: currentColor;\n  stroke-width: 1.7;\n  stroke-linecap: round;\n  stroke-linejoin: round;\n}\n\n.sign-in-button[_ngcontent-%COMP%]:hover {\n  transform: translateY(-1px);\n  box-shadow: 0 12px 24px rgba(255, 95, 0, .19);\n}\n\n.form-message[_ngcontent-%COMP%] {\n  margin: 10px 0 0;\n  color: #6f7890;\n  font-size: 12px;\n  text-align: center;\n}\n\n.divider-row[_ngcontent-%COMP%] {\n  margin: 29px 0 22px;\n  display: grid;\n  grid-template-columns: 1fr auto 1fr;\n  align-items: center;\n  gap: 20px;\n}\n\n.divider-row[_ngcontent-%COMP%]   span[_ngcontent-%COMP%] {\n  height: 1px;\n  background: #e1e4e9;\n}\n\n.divider-row[_ngcontent-%COMP%]   p[_ngcontent-%COMP%] {\n  margin: 0;\n  color: #788198;\n  font-size: 14px;\n}\n\n.social-buttons[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  justify-content: center;\n  gap: 26px;\n}\n\n.social-button[_ngcontent-%COMP%] {\n  width: 67px;\n  height: 54px;\n  display: grid;\n  place-items: center;\n  border: 1px solid #dde2ea;\n  border-radius: 10px;\n  background: #ffffff;\n  box-shadow: 0 2px 7px rgba(38, 47, 80, .03);\n}\n\n.social-button[_ngcontent-%COMP%]:hover {\n  border-color: #ffb188;\n  background: #fffaf7;\n}\n\n.google-g[_ngcontent-%COMP%] {\n  font-size: 24px;\n  font-weight: 900;\n  background: conic-gradient(from -45deg, #4285f4 0 25%, #34a853 0 42%, #fbbc05 0 68%, #ea4335 0 83%, #4285f4 0);\n  -webkit-background-clip: text;\n  background-clip: text;\n  color: transparent;\n}\n\n.apple-button[_ngcontent-%COMP%]   svg[_ngcontent-%COMP%] {\n  width: 25px;\n  height: 25px;\n  fill: #050505;\n}\n\n.microsoft-logo[_ngcontent-%COMP%] {\n  width: 22px;\n  height: 22px;\n  display: grid;\n  grid-template-columns: 1fr 1fr;\n  gap: 2px;\n}\n\n.microsoft-logo[_ngcontent-%COMP%]   i[_ngcontent-%COMP%]:nth-child(1) { background: #f35325; }\n.microsoft-logo[_ngcontent-%COMP%]   i[_ngcontent-%COMP%]:nth-child(2) { background: #81bc06; }\n.microsoft-logo[_ngcontent-%COMP%]   i[_ngcontent-%COMP%]:nth-child(3) { background: #05a6f0; }\n.microsoft-logo[_ngcontent-%COMP%]   i[_ngcontent-%COMP%]:nth-child(4) { background: #ffba08; }\n\n.create-row[_ngcontent-%COMP%] {\n  margin: 28px 0 0;\n  text-align: center;\n  color: #768096;\n  font-size: 14px;\n}\n\n.create-row[_ngcontent-%COMP%]   a[_ngcontent-%COMP%] {\n  margin-left: 5px;\n  color: #ff5b00;\n  font-weight: 550;\n  text-decoration: none;\n}\n\n.back-home[_ngcontent-%COMP%] {\n  display: block;\n  width: fit-content;\n  margin: 24px auto 0;\n  color: #8b92a4;\n  font-size: 11px;\n  text-decoration: none;\n}\n\n.back-home[_ngcontent-%COMP%]:hover {\n  color: #ff5b00;\n}\n\n.sr-only[_ngcontent-%COMP%] {\n  position: absolute !important;\n  width: 1px !important;\n  height: 1px !important;\n  padding: 0 !important;\n  margin: -1px !important;\n  overflow: hidden !important;\n  clip: rect(0, 0, 0, 0) !important;\n  white-space: nowrap !important;\n  border: 0 !important;\n}\n\n@media (min-width: 1665px) {\n  .form-panel[_ngcontent-%COMP%] {\n    padding-left: clamp(66px, 4vw, 84px);\n    padding-right: clamp(62px, 4vw, 84px);\n  }\n\n  .login-card[_ngcontent-%COMP%] {\n    max-width: 650px;\n    margin-left: auto;\n    margin-right: auto;\n  }\n}\n\n@media (max-width: 1180px) {\n  .login-page[_ngcontent-%COMP%] {\n    grid-template-columns: 50% 50%;\n  }\n\n  .form-panel[_ngcontent-%COMP%] {\n    padding: 26px 36px 36px;\n  }\n\n  .login-card[_ngcontent-%COMP%] {\n    padding: 42px 38px 34px;\n  }\n}\n\n@media (max-width: 900px) {\n  .login-page[_ngcontent-%COMP%] {\n    grid-template-columns: 1fr;\n  }\n\n  .visual-panel[_ngcontent-%COMP%] {\n    display: none;\n  }\n\n  .form-panel[_ngcontent-%COMP%] {\n    min-height: 100vh;\n    padding: 24px;\n  }\n\n  .language-wrap[_ngcontent-%COMP%] {\n    min-height: 62px;\n  }\n\n  .login-card[_ngcontent-%COMP%] {\n    max-width: 620px;\n    min-height: auto;\n    margin: 0 auto;\n  }\n}\n\n@media (max-width: 560px) {\n  .form-panel[_ngcontent-%COMP%] {\n    padding: 15px;\n  }\n\n  .language-wrap[_ngcontent-%COMP%] {\n    min-height: 57px;\n  }\n\n  .language-button[_ngcontent-%COMP%] {\n    width: 132px;\n    height: 40px;\n    font-size: 13px;\n  }\n\n  .login-card[_ngcontent-%COMP%] {\n    padding: 30px 22px 28px;\n    border-radius: 20px;\n  }\n\n  h1[_ngcontent-%COMP%] {\n    font-size: 30px;\n  }\n\n  .subtitle[_ngcontent-%COMP%] {\n    margin-bottom: 28px;\n  }\n\n  .form-options[_ngcontent-%COMP%] {\n    align-items: flex-start;\n    flex-direction: column;\n    gap: 12px;\n  }\n\n  .social-buttons[_ngcontent-%COMP%] {\n    gap: 14px;\n  }\n\n  .social-button[_ngcontent-%COMP%] {\n    width: 62px;\n  }\n}"] });
}
(() => { (typeof ngDevMode === "undefined" || ngDevMode) && i0.ɵsetClassMetadata(Login, [{
        type: Component,
        args: [{ selector: 'app-login', standalone: true, imports: [FormsModule, RouterLink], template: "<main class=\"login-page\">\n  <section class=\"visual-panel\" aria-label=\"Event Parking Reservation System\">\n    <span class=\"sr-only\">\n      Event Parking Reservation System. Book events, reserve parking, and manage your tickets seamlessly.\n    </span>\n  </section>\n  <section class=\"form-panel\">\n    <div class=\"language-wrap\">\n      <button\n        class=\"language-button\"\n        type=\"button\"\n        aria-haspopup=\"menu\"\n        [attr.aria-expanded]=\"languageOpen()\"\n        (click)=\"languageOpen.set(!languageOpen())\"\n      >\n        <svg viewBox=\"0 0 24 24\" aria-hidden=\"true\">\n          <circle cx=\"12\" cy=\"12\" r=\"9\"></circle>\n          <path d=\"M3 12h18M12 3a15 15 0 0 1 0 18M12 3a15 15 0 0 0 0 18\"></path>\n        </svg>\n        <span>English</span>\n        <svg class=\"chevron\" viewBox=\"0 0 24 24\" aria-hidden=\"true\">\n          <path d=\"m8 10 4 4 4-4\"></path>\n        </svg>\n      </button>\n\n      @if (languageOpen()) {\n        <div class=\"language-menu\" role=\"menu\">\n          <button type=\"button\" role=\"menuitem\" (click)=\"languageOpen.set(false)\">English</button>\n        </div>\n      }\n    </div>\n\n    <section class=\"login-card\" aria-labelledby=\"login-title\">\n      <div class=\"lock-badge\" aria-hidden=\"true\">\n        <svg viewBox=\"0 0 24 24\">\n          <rect x=\"6\" y=\"10\" width=\"12\" height=\"10\" rx=\"2\"></rect>\n          <path d=\"M9 10V7a3 3 0 0 1 6 0v3\"></path>\n        </svg>\n      </div>\n\n      <h1 id=\"login-title\">Welcome Back</h1>\n      <p class=\"subtitle\">Sign in to continue to your account</p>\n\n      <form (ngSubmit)=\"submit()\" novalidate>\n        <label class=\"field-label\" for=\"login-id\">Email or Username</label>\n        <div class=\"input-shell\">\n          <svg class=\"field-icon\" viewBox=\"0 0 24 24\" aria-hidden=\"true\">\n            <rect x=\"3\" y=\"5\" width=\"18\" height=\"14\" rx=\"2\"></rect>\n            <path d=\"m4 7 8 6 8-6\"></path>\n          </svg>\n          <input\n            id=\"login-id\"\n            name=\"emailOrUsername\"\n            type=\"text\"\n            autocomplete=\"username\"\n            placeholder=\"Enter your email or username\"\n            [(ngModel)]=\"emailOrUsername\"\n          >\n        </div>\n\n        <label class=\"field-label password-label\" for=\"login-password\">Password</label>\n        <div class=\"input-shell\">\n          <svg class=\"field-icon\" viewBox=\"0 0 24 24\" aria-hidden=\"true\">\n            <rect x=\"5\" y=\"10\" width=\"14\" height=\"10\" rx=\"2\"></rect>\n            <path d=\"M8 10V7a4 4 0 0 1 8 0v3\"></path>\n          </svg>\n          <input\n            id=\"login-password\"\n            name=\"password\"\n            [type]=\"passwordVisible() ? 'text' : 'password'\"\n            autocomplete=\"current-password\"\n            placeholder=\"Enter your password\"\n            [(ngModel)]=\"password\"\n          >\n          <button\n            class=\"eye-button\"\n            type=\"button\"\n            [attr.aria-label]=\"passwordVisible() ? 'Hide password' : 'Show password'\"\n            (click)=\"togglePassword()\"\n          >\n            <svg viewBox=\"0 0 24 24\" aria-hidden=\"true\">\n              <path d=\"M2.5 12s3.5-6 9.5-6 9.5 6 9.5 6-3.5 6-9.5 6-9.5-6-9.5-6Z\"></path>\n              <circle cx=\"12\" cy=\"12\" r=\"2.5\"></circle>\n            </svg>\n          </button>\n        </div>\n\n        <div class=\"form-options\">\n          <label class=\"remember-control\">\n            <input\n              class=\"sr-only\"\n              type=\"checkbox\"\n              [checked]=\"rememberMe()\"\n              (change)=\"toggleRemember()\"\n            >\n            <span class=\"check-box\" [class.checked]=\"rememberMe()\">\n              @if (rememberMe()) { \u2713 }\n            </span>\n            <span>Remember me</span>\n          </label>\n\n          <a class=\"forgot-link\" href=\"#\">Forgot Password?</a>\n        </div>\n\n        <button class=\"sign-in-button\" type=\"submit\">\n          <span>Sign In</span>\n          <svg viewBox=\"0 0 24 24\" aria-hidden=\"true\">\n            <path d=\"M5 12h14M13 6l6 6-6 6\"></path>\n          </svg>\n        </button>\n\n        @if (message()) {\n          <p class=\"form-message\" role=\"status\">{{ message() }}</p>\n        }\n      </form>\n\n      <div class=\"divider-row\" aria-hidden=\"true\">\n        <span></span>\n        <p>or continue with</p>\n        <span></span>\n      </div>\n\n      <div class=\"social-buttons\" aria-label=\"Social sign in options\">\n        <button class=\"social-button\" type=\"button\" aria-label=\"Continue with Google\">\n          <span class=\"google-g\">G</span>\n        </button>\n\n        <button class=\"social-button apple-button\" type=\"button\" aria-label=\"Continue with Apple\">\n          <svg viewBox=\"0 0 24 24\" aria-hidden=\"true\">\n            <path d=\"M16.7 12.7c0-2.5 2-3.7 2.1-3.8-1.2-1.7-3-1.9-3.7-1.9-1.6-.2-3.1.9-3.9.9-.8 0-2-.9-3.4-.9-1.7 0-3.4 1-4.3 2.6-1.9 3.2-.5 8 1.3 10.6.9 1.3 2 2.8 3.5 2.7 1.4-.1 1.9-.9 3.7-.9 1.7 0 2.2.9 3.7.9 1.6 0 2.5-1.3 3.4-2.6 1-1.5 1.5-3 1.5-3.1-.1 0-2.9-1.1-2.9-4.5Zm-2.6-7.4c.8-.9 1.3-2.2 1.2-3.3-1.1.1-2.4.7-3.2 1.6-.7.8-1.3 2.1-1.2 3.2 1.2.1 2.4-.6 3.2-1.5Z\"></path>\n          </svg>\n        </button>\n\n        <button class=\"social-button\" type=\"button\" aria-label=\"Continue with Microsoft\">\n          <span class=\"microsoft-logo\" aria-hidden=\"true\">\n            <i></i><i></i><i></i><i></i>\n          </span>\n        </button>\n      </div>\n\n      <p class=\"create-row\">\n        Don\u2019t have an account?\n        <a href=\"#\">Create Account</a>\n      </p>\n\n      <a class=\"back-home\" routerLink=\"/\">\u2190 Back to Eventora</a>\n    </section>\n  </section>\n</main>\n", styles: [":host {\n  display: block;\n  min-height: 100vh;\n}\n\n* {\n  box-sizing: border-box;\n}\n\n.login-page {\n  min-height: 100vh;\n  display: grid;\n  grid-template-columns: 54.7% 45.3%;\n  background: #ffffff;\n  color: #10162f;\n  font-family: Inter, Arial, sans-serif;\n}\n\n.visual-panel {\n  position: relative;\n  min-height: 100vh;\n  background: #130b31 url('/assets/login-left-visual.jpg') center / 100% 100% no-repeat;\n  overflow: hidden;\n}\n\n.form-panel {\n  position: relative;\n  min-height: 100vh;\n  padding: 30px 62px 42px 66px;\n  background:\n    radial-gradient(circle at 77% 72%, rgba(255, 116, 24, .025), transparent 34%),\n    #ffffff;\n}\n\n.language-wrap {\n  position: relative;\n  display: flex;\n  justify-content: flex-end;\n  min-height: 61px;\n}\n\n.language-button {\n  width: 145px;\n  height: 43px;\n  padding: 0 14px 0 16px;\n  display: grid;\n  grid-template-columns: 20px 1fr 16px;\n  align-items: center;\n  gap: 9px;\n  border: 1px solid #dde1e8;\n  border-radius: 14px;\n  color: #131a31;\n  background: #ffffff;\n  font-size: 14px;\n  font-weight: 650;\n  box-shadow: 0 3px 10px rgba(42, 52, 85, .03);\n}\n\n.language-button svg {\n  width: 19px;\n  height: 19px;\n  fill: none;\n  stroke: currentColor;\n  stroke-width: 1.8;\n  stroke-linecap: round;\n  stroke-linejoin: round;\n}\n\n.language-button .chevron {\n  width: 15px;\n  height: 15px;\n  justify-self: end;\n}\n\n.language-menu {\n  position: absolute;\n  z-index: 5;\n  top: 48px;\n  right: 0;\n  width: 145px;\n  padding: 6px;\n  border: 1px solid #e4e7ed;\n  border-radius: 12px;\n  background: #ffffff;\n  box-shadow: 0 14px 35px rgba(31, 39, 74, .12);\n}\n\n.language-menu button {\n  width: 100%;\n  padding: 9px 11px;\n  border: 0;\n  border-radius: 8px;\n  text-align: left;\n  background: transparent;\n}\n\n.language-menu button:hover {\n  background: #fff4ec;\n}\n\n.login-card {\n  width: 100%;\n  min-height: 768px;\n  margin-top: 0;\n  padding: 47px 48px 38px;\n  border: 1px solid #f0e5df;\n  border-radius: 24px;\n  background: rgba(255, 255, 255, .98);\n  box-shadow: 0 17px 40px rgba(33, 41, 75, .055);\n}\n\n.lock-badge {\n  width: 54px;\n  height: 54px;\n  margin-bottom: 22px;\n  display: grid;\n  place-items: center;\n  border-radius: 50%;\n  color: #ff6508;\n  background: #fff3ec;\n}\n\n.lock-badge svg {\n  width: 24px;\n  height: 24px;\n  fill: none;\n  stroke: currentColor;\n  stroke-width: 2;\n  stroke-linecap: round;\n  stroke-linejoin: round;\n}\n\nh1 {\n  margin: 0;\n  color: #10162f;\n  font-size: 36px;\n  line-height: 1.08;\n  font-weight: 800;\n  letter-spacing: -1.15px;\n}\n\n.subtitle {\n  margin: 11px 0 34px;\n  color: #737b94;\n  font-size: 15px;\n  line-height: 1.45;\n}\n\n.field-label {\n  display: block;\n  margin-bottom: 10px;\n  color: #12182d;\n  font-size: 13px;\n  font-weight: 700;\n}\n\n.password-label {\n  margin-top: 23px;\n}\n\n.input-shell {\n  height: 53px;\n  display: flex;\n  align-items: center;\n  gap: 13px;\n  padding: 0 16px;\n  border: 1px solid #d9dee8;\n  border-radius: 10px;\n  background: #ffffff;\n  transition: border-color .2s ease, box-shadow .2s ease;\n}\n\n.input-shell:focus-within {\n  border-color: #ff8a45;\n  box-shadow: 0 0 0 3px rgba(255, 105, 20, .09);\n}\n\n.field-icon {\n  flex: 0 0 21px;\n  width: 21px;\n  height: 21px;\n  fill: none;\n  stroke: #7a849a;\n  stroke-width: 1.8;\n  stroke-linecap: round;\n  stroke-linejoin: round;\n}\n\n.input-shell input {\n  min-width: 0;\n  width: 100%;\n  height: 100%;\n  padding: 0;\n  border: 0;\n  outline: 0;\n  color: #182039;\n  background: transparent;\n  font-size: 14px;\n}\n\n.input-shell input::placeholder {\n  color: #8c94a9;\n}\n\n.eye-button {\n  flex: 0 0 32px;\n  width: 32px;\n  height: 32px;\n  display: grid;\n  place-items: center;\n  border: 0;\n  border-radius: 8px;\n  color: #737d93;\n  background: transparent;\n}\n\n.eye-button:hover {\n  background: #f7f8fa;\n}\n\n.eye-button svg {\n  width: 21px;\n  height: 21px;\n  fill: none;\n  stroke: currentColor;\n  stroke-width: 1.8;\n  stroke-linecap: round;\n  stroke-linejoin: round;\n}\n\n.form-options {\n  margin: 22px 0 25px;\n  display: flex;\n  align-items: center;\n  justify-content: space-between;\n  gap: 20px;\n}\n\n.remember-control {\n  display: inline-flex;\n  align-items: center;\n  gap: 10px;\n  color: #31384f;\n  font-size: 14px;\n  cursor: pointer;\n  user-select: none;\n}\n\n.check-box {\n  width: 19px;\n  height: 19px;\n  display: grid;\n  place-items: center;\n  border: 1px solid #d4d9e2;\n  border-radius: 4px;\n  color: #ffffff;\n  background: #ffffff;\n  font-size: 12px;\n  font-weight: 800;\n}\n\n.check-box.checked {\n  border-color: #ff6908;\n  background: #ff6908;\n}\n\n.forgot-link {\n  color: #ff5b00;\n  font-size: 14px;\n  text-decoration: none;\n}\n\n.forgot-link:hover,\n.create-row a:hover {\n  text-decoration: underline;\n}\n\n.sign-in-button {\n  width: 100%;\n  height: 54px;\n  padding: 0 32px;\n  display: grid;\n  grid-template-columns: 1fr auto 1fr;\n  align-items: center;\n  border: 0;\n  border-radius: 10px;\n  color: #ffffff;\n  background: linear-gradient(90deg, #ff7a00 0%, #ff5600 100%);\n  box-shadow: 0 8px 18px rgba(255, 95, 0, .13);\n  font-size: 16px;\n  font-weight: 750;\n  transition: transform .2s ease, box-shadow .2s ease;\n}\n\n.sign-in-button span {\n  grid-column: 2;\n}\n\n.sign-in-button svg {\n  grid-column: 3;\n  justify-self: end;\n  width: 24px;\n  height: 24px;\n  fill: none;\n  stroke: currentColor;\n  stroke-width: 1.7;\n  stroke-linecap: round;\n  stroke-linejoin: round;\n}\n\n.sign-in-button:hover {\n  transform: translateY(-1px);\n  box-shadow: 0 12px 24px rgba(255, 95, 0, .19);\n}\n\n.form-message {\n  margin: 10px 0 0;\n  color: #6f7890;\n  font-size: 12px;\n  text-align: center;\n}\n\n.divider-row {\n  margin: 29px 0 22px;\n  display: grid;\n  grid-template-columns: 1fr auto 1fr;\n  align-items: center;\n  gap: 20px;\n}\n\n.divider-row span {\n  height: 1px;\n  background: #e1e4e9;\n}\n\n.divider-row p {\n  margin: 0;\n  color: #788198;\n  font-size: 14px;\n}\n\n.social-buttons {\n  display: flex;\n  align-items: center;\n  justify-content: center;\n  gap: 26px;\n}\n\n.social-button {\n  width: 67px;\n  height: 54px;\n  display: grid;\n  place-items: center;\n  border: 1px solid #dde2ea;\n  border-radius: 10px;\n  background: #ffffff;\n  box-shadow: 0 2px 7px rgba(38, 47, 80, .03);\n}\n\n.social-button:hover {\n  border-color: #ffb188;\n  background: #fffaf7;\n}\n\n.google-g {\n  font-size: 24px;\n  font-weight: 900;\n  background: conic-gradient(from -45deg, #4285f4 0 25%, #34a853 0 42%, #fbbc05 0 68%, #ea4335 0 83%, #4285f4 0);\n  -webkit-background-clip: text;\n  background-clip: text;\n  color: transparent;\n}\n\n.apple-button svg {\n  width: 25px;\n  height: 25px;\n  fill: #050505;\n}\n\n.microsoft-logo {\n  width: 22px;\n  height: 22px;\n  display: grid;\n  grid-template-columns: 1fr 1fr;\n  gap: 2px;\n}\n\n.microsoft-logo i:nth-child(1) { background: #f35325; }\n.microsoft-logo i:nth-child(2) { background: #81bc06; }\n.microsoft-logo i:nth-child(3) { background: #05a6f0; }\n.microsoft-logo i:nth-child(4) { background: #ffba08; }\n\n.create-row {\n  margin: 28px 0 0;\n  text-align: center;\n  color: #768096;\n  font-size: 14px;\n}\n\n.create-row a {\n  margin-left: 5px;\n  color: #ff5b00;\n  font-weight: 550;\n  text-decoration: none;\n}\n\n.back-home {\n  display: block;\n  width: fit-content;\n  margin: 24px auto 0;\n  color: #8b92a4;\n  font-size: 11px;\n  text-decoration: none;\n}\n\n.back-home:hover {\n  color: #ff5b00;\n}\n\n.sr-only {\n  position: absolute !important;\n  width: 1px !important;\n  height: 1px !important;\n  padding: 0 !important;\n  margin: -1px !important;\n  overflow: hidden !important;\n  clip: rect(0, 0, 0, 0) !important;\n  white-space: nowrap !important;\n  border: 0 !important;\n}\n\n@media (min-width: 1665px) {\n  .form-panel {\n    padding-left: clamp(66px, 4vw, 84px);\n    padding-right: clamp(62px, 4vw, 84px);\n  }\n\n  .login-card {\n    max-width: 650px;\n    margin-left: auto;\n    margin-right: auto;\n  }\n}\n\n@media (max-width: 1180px) {\n  .login-page {\n    grid-template-columns: 50% 50%;\n  }\n\n  .form-panel {\n    padding: 26px 36px 36px;\n  }\n\n  .login-card {\n    padding: 42px 38px 34px;\n  }\n}\n\n@media (max-width: 900px) {\n  .login-page {\n    grid-template-columns: 1fr;\n  }\n\n  .visual-panel {\n    display: none;\n  }\n\n  .form-panel {\n    min-height: 100vh;\n    padding: 24px;\n  }\n\n  .language-wrap {\n    min-height: 62px;\n  }\n\n  .login-card {\n    max-width: 620px;\n    min-height: auto;\n    margin: 0 auto;\n  }\n}\n\n@media (max-width: 560px) {\n  .form-panel {\n    padding: 15px;\n  }\n\n  .language-wrap {\n    min-height: 57px;\n  }\n\n  .language-button {\n    width: 132px;\n    height: 40px;\n    font-size: 13px;\n  }\n\n  .login-card {\n    padding: 30px 22px 28px;\n    border-radius: 20px;\n  }\n\n  h1 {\n    font-size: 30px;\n  }\n\n  .subtitle {\n    margin-bottom: 28px;\n  }\n\n  .form-options {\n    align-items: flex-start;\n    flex-direction: column;\n    gap: 12px;\n  }\n\n  .social-buttons {\n    gap: 14px;\n  }\n\n  .social-button {\n    width: 62px;\n  }\n}\n"] }]
    }], null, null); })();
(() => { (typeof ngDevMode === "undefined" || ngDevMode) && i0.ɵsetClassDebugInfo(Login, { className: "Login", filePath: "src/app/pages/login/login.ts", lineNumber: 12 }); })();
