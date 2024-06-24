import React, { useEffect, useState } from "react";
import './ListCustomers.css';
import { serverUri, apiEndpoint, theme, fetchWithAuth } from  "../../App.jsx";
import {CircularProgress, ThemeProvider} from "@mui/material";

const ListCustomers = () => {
    const [customers, setCustomers] = useState([]);
    const [customerUpdating, setCustomerUpdating] = useState(false);
    const fetchInfo = async () => {
        // Check if customer data is cached locally and use that if available, always refetch regardless to ensure up to date data.
        let customerInfo = sessionStorage.getItem("customerInfo");
        if (customerInfo !== null) {
            setCustomers(JSON.parse(customerInfo));
        }
        else {
            setCustomerUpdating(true);
        }
        
        const response = await fetchWithAuth(`${serverUri}${apiEndpoint}/GetCustomers`);
        const data = await response.json();
        sessionStorage.setItem("customerInfo", JSON.stringify(data));
        setCustomers(data);
        
        console.log("Updated customer data")
        setCustomerUpdating(false);
    };

    useEffect(() => {
        fetchInfo();
    }, []);

    return (
        <ThemeProvider theme={theme}>

            <div className={"page-headers"}>
                <h1 className="title">Customers</h1>
            </div>
            <div className="list-customer">
                <div className="listcustomer-format listcustomer-header">
                    <p className={"col-1"}>#</p>
                    <p className={"col-2"}>Name</p>
                    <p className={"col-3"}>Email</p>
                    <p className={"col-4"}>Stripe ID</p>
                    <p className={"col-5"}>System ID</p>
                </div>

                {customerUpdating && <div style={{
                    display: 'flex',
                    flexDirection: 'column',
                    justifyContent: 'center',
                    alignItems: 'center',
                    height: '100vh'
                }}>
                    <CircularProgress/>
                    <span>Loading...</span>
                </div>}

                {!customerUpdating ? (
                    <div className="listcustomer-allcustomers">
                        {customers.map((customer, index) => (
                            <div key={index} className="listcustomer-format listcustomer-item"
                                 onClick={(e) => handleOrderClick(e, order.id)}>
                                <p className={"col-1"}>{index + 1}</p>
                                <p className={"col-2"}>{customer.firstName} {customer.lastName}</p>
                                <p className={"col-3"}>{customer.email}</p>
                                <p className={"col-4"}>{customer.stripeId}</p>
                                <p className={"col-5"}>{customer.id}</p>
                            </div>
                        ))}
                        {(customers.length === 0 && !customerUpdating) && <p className="no-customers-message">No customers available</p>}
                    </div>
                ) : null}
            </div>
        </ThemeProvider>
    );
}

export default ListCustomers;
