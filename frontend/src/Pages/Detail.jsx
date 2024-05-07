 

 
/*const Detail = () => {
  const user = {
    name: 'John Doe',
    email: 'john@example.com',
    address: '1234 Sunset Blvd'
  };

  const purchases = [
    { date: '2021-01-01', itemName: 'Apple MacBook Pro', amount: 1200 },
    { date: '2021-02-15', itemName: 'Logitech Mouse', amount: 25 }
  ];

  return (
    <div className="App">
      <UserDetails user={user} purchases={purchases} />
    </div>
  );
};

export default Detail;*/

 

import React, { useState, useEffect } from 'react';
import UserDetails from '../Components/Usedetail/Usedetail';

function Detail() {
  const [user, setUser] = useState(null);
  const [purchases, setPurchases] = useState([]); // Initialize as an empty array
  const [error, setError] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const userId = localStorage.getItem('Id');
    const userUrl = `http://127.0.0.1:3000/api/shop/GetCustomerByID/${userId}`;
    const purchasesUrl = `http://127.0.0.1:3000/api/shop/GetCustomerOrders/bb8150ef-f138-4590-8452-4fee498f50e7`;/*test id in here*/

    Promise.all([
        fetch(userUrl),
        fetch(purchasesUrl)
      ]).then(async ([userResponse, purchasesResponse]) => {
        if (!userResponse.ok) throw new Error('Error fetching user');
        if (!purchasesResponse.ok) throw new Error('Error fetching purchases');

        const userData = await userResponse.json();
        const purchasesData = await purchasesResponse.json();

        setUser({
          name: userData.firstName + " " + userData.lastName,
          email: userData.email,
        });

        setPurchases(purchasesData.map(purchase => ({
          trackingNumber: purchase.trackingNumber,
          id: purchase.id,
          packageReference: purchase.packageReference
        })));  
        setLoading(false);
      }).catch(error => {
        setError(error.message);
        setLoading(false);
      });
  }, []);

  if (loading) return <div>Loading...</div>;
  if (error) return <div>Error: {error}</div>;

  return (
    <div className="App">
      <UserDetails user={user} purchases={purchases} />
    </div>
  )
}

export default Detail;
