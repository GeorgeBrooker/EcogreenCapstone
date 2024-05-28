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
    // Manage verification code input 
    const [showVerification, setShowVerification] = useState(false); // Track whether the verification code input should be shown
    const [verificationCode, setVerificationCode] = useState(""); // Track the verification code for sign up
    const [verificationError, setVerificationError] = useState(""); // Track any errors with the verification code 

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
            else if (response.status === 401) {
                alert("Login failed: Incorrect email or password");
                return
            }
            else {
                console.error("Login failed for user with email: ", formData.email, " and password: ", formData.password);
                alert("Login failed for an unknown reason");
                return
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
            console.log("Signup successful");
            localStorage.removeItem("auth-token");
            localStorage.removeItem("refresh-token");
            console.log("Checking email confirmation");
            
            const userConfirmed = await response.json().then(data => data.confirmed);
            if (userConfirmed) {
                console.log("User confirmed, logging in")
                await login();
            }
            // If the user is not confirmed, show the verification code input
            else {
                console.log("User not confirmed, showing verification code input");
                setShowVerification(true);
            }
            
        }else{
            console.error("Signup failed");
            alert("Signup failed");
        }
    };
    
    const handleUserVerification = async () => {
        console.log("Handling user verification");
        const response = await fetch(serverUri + '/api/auth/ConfirmCustomer', {
            method: "POST",
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                "Email": formData.email,
                "Code": verificationCode
            }),
        });
        if (response.ok) {
            console.log("User verified, logging in");
            await login();
        }else{
            console.error("User verification failed");
            setVerificationError(`Verification code is incorrect ${response.statusText}`);
        }
    }
    const handleCancelUserVerification = () => {
        console.log("Verification cancelled");
        setShowVerification(false);
    }

    return (
        <div className="loginsignup">
            {showVerification && (
                <div className="verification-box">
                    <div className="verification-box-inner">
                        <h1>Verify Your Email</h1>
                        <p>We have sent a 6 digit verification code to your email.<br/><br/> Please enter it below:</p>
                        <input type="text" value={verificationCode}
                               onChange={(e) => setVerificationCode(e.target.value)}/>
                        {verificationError && <p className="error">{verificationError}</p>}
                        <button className="handleVerification" onClick={handleUserVerification}>Confirm</button>
                        <button className="cancelVerification" onClick={handleCancelUserVerification}>Cancel</button>
                    </div>
                </div>
            )}
            <div className="loginsignup-container">
                <h1>{state}</h1>
                <div className="loginsignup-fields">
                    {state === "Sign Up" &&
                        <>
                            <input name="firstName" value={formData.firstName} onChange={changeHandler} type="text"
                                   placeholder="First name"/>
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