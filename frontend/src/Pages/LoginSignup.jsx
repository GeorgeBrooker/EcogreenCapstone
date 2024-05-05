import React, { useState } from "react"
import './CSS/LoginSignup.css'

const LoginSignup = () => {

    const [state, setState] = useState("Login");
    const [formData,setFormData] = useState({
        firstName:"",
        lastName:"",
        password:"",
        email:""
    })

    const changeHandler = (e)=>{
        setFormData({...formData,[e.target.name]:e.target.value})
    }

    const login = async () =>{
        console.log("Login Function Executed",formData);
        // Create Json Object with format CustomerInput (Backend/Dtos) to send to the server TODO THIS NEEDS TO CHANGE FOR LOGIN. LOGIN VIA EMAIL AND PASSWORD (no username)
        let customerInput = {
            Fname:formData.firstName,
            Lname:formData.lastName,
            Email:formData.email,
            password:formData.password
        };
        
        let responseData;
        await fetch('http://localhost:4000/login',{
            method:'POST',
            headers:{
                Accept:'application/form-data',
                'Content-Type':'application/json',
            },
            body: JSON.stringify(customerInput),
        }).then((response)=> response.json()).then((data)=>responseData=data)

        if(responseData.success){
            localStorage.setItem('auth-token',responseData.token);
            window.localtion.replace("/home");
        }
        else{
            alert(responseData.errors)
        }
    }

    const signup = async () =>{
        console.log("Signup Function Executed",formData);
        // Create Json Object with format CustomerInput (Backend/Dtos) to send to the server 
        let customerInput = {
            "Fname":formData.firstName,
            "Lname":formData.lastName,
            "Email":formData.email,
            "Pass":formData.password
        };
        let responseData;
        await fetch('http://127.0.0.1:3000/api/shop/AddCustomer',{
            method:'POST',
            headers:{
                Accept:'application/form-data',
                'Content-Type':'application/json',
            },
            body: JSON.stringify(customerInput),
        }).then((response)=> response.json()).then((data)=>responseData=data)

        if(responseData.success){
            localStorage.setItem('auth-token',responseData.token);
            window.location.replace("/home");
        }
        else{
            alert(responseData.errors)
        }
    }

    return (
        <div className="loginsignup">
            <div className="loginsignup-container">
                <h1>{state}</h1>
                <div className="loginsignup-fields">
                    {/* If the state is "Sign Up" then show the first name and last name fields. TODO Find a way to wrap both fname,lname into one comparison */} 
                    {state === "Sign Up" ?
                        <input name="firstName" value={formData.firstName} onChange={changeHandler} type="text"
                               placeholder="First name"/>:<></>}
                    {state === "Sign Up" ?
                        <input name="lastName" value={formData.lastName} onChange={changeHandler} type="text"
                               placeholder="Last name"/>:<></>}
                    <input name="email" value={formData.email} onChange={changeHandler} type="email" placeholder="Email" />
                    <input name="password" value={formData.password} onChange={changeHandler} type="password" placeholder="Password" />
                </div>
                <button onClick={()=>{state==="Login"?login():signup()}}>Continue</button>
                {state==="Sign Up"?
                <p className="loginsignup-login">Already have an account? <span onClick={()=>{setState("Login")}}>Login here</span></p>:
                <p className="loginsignup-login">Create an account? <span onClick={()=>{setState("Sign Up")}}>Click here</span></p>
                }
                <div className="loginsignup-agree">
                    <input type="checkbox" name="" id="" />
                    <p>By continuing, I agree to the terms & privacy policy.</p>
                </div>
            </div>
        </div>
    )
}

export default LoginSignup