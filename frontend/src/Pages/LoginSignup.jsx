/* eslint-disable no-use-before-define */
 
import React, {useContext, useState} from "react"
import { ShopContext } from '../Context/ShopContext'
import './CSS/LoginSignup.css'

const LoginSignup = () => {
    const {serverUri} = useContext(ShopContext);
    const {checkLogin} = useContext(ShopContext);
    
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
        // Do not check terms agreement for login (customer has already agreed on sign up)
        if (!agreedToTerms && state === "Sign Up") {
            alert("You must agree to the terms and privacy policy before continuing.");
            return false;
        }
        return true;
    };

    const login = async () => {
        let token = localStorage.getItem('id-token');
        let access_token = localStorage.getItem('auth-token');
        let refresh_token = localStorage.getItem('refresh-token');
        
        let customerInput = {
            "Fname": "None",
            "Lname": "None",
            "Email": formData.email,
            "Pass": formData.password
        };
        
        // Get token if no token is stored
        if(!access_token || !refresh_token || !token) {
            const response = await fetch(serverUri + '/api/auth/CustomerLogin', {
                method: "POST",
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(customerInput),
            });
            if (response.ok) {
                const data = await response.json();
                token = data.token;
                access_token = data.accessToken;
                refresh_token = data.refreshToken;
                
                localStorage.setItem('id-token', token);
                localStorage.setItem('auth-token', access_token);
                localStorage.setItem('refresh-token', refresh_token);
                
                console.log("Got login tokens:\n", token, "\n\n", access_token, "\n\n", refresh_token);
            }
        }
        
        // Validate token
        const response = await fetch(serverUri + '/api/auth/ValidateCustomer', {
            method: "GET",
            headers: {
                'Authorization': 'Bearer ' + btoa(localStorage.getItem('auth-token') + ':' + localStorage.getItem('refresh-token')),
            },
        });
        if (!response.ok) {
            console.error("Login failed for user with email: ", formData.email, " and password: ", formData.password);
            localStorage.removeItem('auth-token');
            localStorage.removeItem('refresh-token');
            localStorage.removeItem('id-token');
            alert("Login failed");
            return
        }
        
        // Store user session in session storage
        const userSession = await response.json();
        sessionStorage.setItem('Email', userSession.email);
        sessionStorage.setItem('Fname', userSession.fname);
        sessionStorage.setItem('Lname', userSession.lname);
        sessionStorage.setItem('Id', userSession.id);
         
        console.log(
            `login successful, user information stored in session storage
            \nEmail=${sessionStorage.getItem("Email")}
            \nFname=${sessionStorage.getItem("Fname")}
            \nLname=${sessionStorage.getItem("Lname")}
            \nId=${sessionStorage.getItem("Id")}`
        );
        
        // Handle successful login
        alert("Login successful!");
        window.location.replace('/home')
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
        
        const response = await fetch(serverUri + '/api/auth/RegisterCustomer', {
            method: "POST",
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(customerInput),
        });
        if (response.ok) {
            // Signup successful, clear any existing auth token and log the user in.
            console.log("Signup successful, logging user in now");
            localStorage.removeItem("auth-token");
            localStorage.removeItem("refresh-token");
            
            await login();
        }else{
            console.error("Signup failed");
            alert("Signup failed");
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
                    <>
                        <div className="loginsignup-agree">
                            <input type="checkbox" checked={agreedToTerms} onChange={termsChangeHandler}/>
                            <p>By continuing, I agree to the terms & privacy policy.</p>
                        </div>
                        <p className="loginsignup-login">Already have an account? <span onClick={() => {setState("Login")}}>Login here</span></p>
                    </> 
                    :
                    <p className="loginsignup-login">Need an account? <span onClick={() => {setState("Sign Up")}}>Sign up here</span></p>
                }
            </div>
        </div>
    )
}

export default LoginSignup