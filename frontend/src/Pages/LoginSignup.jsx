import React, { useState } from "react"
import './CSS/LoginSignup.css'

const LoginSignup = () => {
    const [state, setState] = useState("Login");
    const [formData, setFormData] = useState({
        firstName: "",
        lastName: "",
        password: "",
        email: ""
    });
    const [agreedToTerms, setAgreedToTerms] = useState(false); // Track whether the terms checkbox is checked

    const changeHandler = (e) => {
        setFormData({ ...formData, [e.target.name]: e.target.value });
    };

    const termsChangeHandler = (e) => {  // Handle changes to the terms agreement checkbox
        setAgreedToTerms(e.target.checked);
    };

    const isFormValid = () => {
        // Check that all fields are filled in
        const requiredFields = state === "Login" ? ["email", "password"] : ["firstName", "lastName", "email", "password"];
        for (let field of requiredFields) {
            if (!formData[field]) {
                alert("Please fill in all fields.");
                return false;
            }
        }
        if (!agreedToTerms) {
            alert("You must agree to the terms and privacy policy before continuing.");
            return false;
        }
        return true;
    };

    const login = async () => {
        if (!isFormValid()) return; // Validate form fields and terms agreement before proceeding

        console.log("Login Function Executed", formData);
        let customerInput = {
            email: formData.email,
            password: formData.password
        };

        let responseData;
        await fetch('http://localhost:4000/login', {
            method: 'POST',
            headers: {
                Accept: 'application/json',
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(customerInput),
        }).then((response) => response.json()).then((data) => responseData = data)

        if (responseData.success) {
            localStorage.setItem('auth-token', responseData.token);
            window.location.replace("/home");
        } else {
            alert(responseData.errors);
        }
    };

    const signup = async () => {
        if (!isFormValid()) return; // Validate form fields and terms agreement before proceeding

        console.log("Signup Function Executed", formData);
        let customerInput = {
            "Fname": formData.firstName,
            "Lname": formData.lastName,
            "Email": formData.email,
            "Pass": formData.password
        };
        let responseData;
        await fetch('http://127.0.0.1:3000/api/shop/AddCustomer', {
            method: 'POST',
            headers: {
                Accept: 'application/json',
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(customerInput),
        }).then((response) => response.json()).then((data) => responseData = data)

        if (responseData.success) {
            localStorage.setItem('auth-token', responseData.token);
            window.location.replace("/home");
        } else {
            alert(responseData.errors);
        }
    };

    return (
        <div className="loginsignup">
            <div className="loginsignup-container">
                <h1>{state}</h1>
                <div className="loginsignup-fields">
                    {state === "Sign Up" &&
                        <>
                            <input name="firstName" value={formData.firstName} onChange={changeHandler} type="text"
                                placeholder="First name" />
                            <input name="lastName" value={formData.lastName} onChange={changeHandler} type="text"
                                placeholder="Last name" />
                        </>
                    }
                    <input name="email" value={formData.email} onChange={changeHandler} type="email" placeholder="Email" />
                    <input name="password" value={formData.password} onChange={changeHandler} type="password" placeholder="Password" />
                </div>
                <button onClick={() => { state === "Login" ? login() : signup() }}>Continue</button>
                {state === "Sign Up" ?
                    <p className="loginsignup-login">Already have an account? <span onClick={() => { setState("Login") }}>Login here</span></p> :
                    <p className="loginsignup-login">Need an account? <span onClick={() => { setState("Sign Up") }}>Sign up here</span></p>
                }
                <div className="loginsignup-agree">
                    <input type="checkbox" checked={agreedToTerms} onChange={termsChangeHandler} />
                    <p>By continuing, I agree to the terms & privacy policy.</p>
                </div>
            </div>
        </div>
    );
}

export default LoginSignup;
