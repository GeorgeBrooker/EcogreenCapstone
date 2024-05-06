import React from 'react';
import UserDetails from '../Components/Usedetail/Usedetail';

 
const Detail = () => {
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

export default Detail;
