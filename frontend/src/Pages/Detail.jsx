import React, {useState, useEffect, useContext} from 'react';
import UserDetails from '../Components/Usedetail/Usedetail';
import { ShopContext } from '../Context/ShopContext';
// import Footer from '../Components/Footer/Footer';
import './CSS/Detail.css'

const loadUserFromCache = () => {
  const u_email = sessionStorage.getItem('Email');
  const u_fname = sessionStorage.getItem('Fname');
  const u_lname = sessionStorage.getItem('Lname');
  const u_id = sessionStorage.getItem('Id');
  if (u_email === null || u_fname === null || u_lname === null || u_id === null) {
    return null;
  }
  
  return {
    email: u_email,
    fname: u_fname,
    lname: u_lname,
    id: u_id
  };
}
function Detail() {
  const {serverUri} = useContext(ShopContext);
  const [user, setUser] = useState(null);
  const [purchases, setPurchases] = useState([]); // Initialize as an empty array
  const [error, setError] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let cacheUser = loadUserFromCache();
    if (cacheUser !== null) {
      setUser({
        name: cacheUser.fname + " " + cacheUser.lname,
        email: cacheUser.email
      });
      setLoading(false);
    }
    
    const userId = cacheUser.id;
    const userUrl = serverUri + `/api/shop/GetCustomerByID/${userId}`;
    const purchasesUrl = serverUri + `/api/shop/GetCustomerOrders/${userId}`;

    Promise.all([
        fetch(userUrl),
        fetch(purchasesUrl)
      ]).then(async ([userResponse, purchasesResponse]) => {
        if (!userResponse.ok) throw new Error('Error fetching user');
        if (!purchasesResponse.ok) throw new Error('Error fetching purchases');

        const userData = await userResponse.json();
        let purchasesData = await purchasesResponse.json();
        purchasesData = purchasesData.sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt));
        
        setUser({
          name: userData.firstName + " " + userData.lastName,
          email: userData.email,
        });

        setPurchases(purchasesData.map(purchase => ({
          date: purchase.createdAt,
          trackingNumber: purchase.trackingNumber,
          id: purchase.id,
          status: purchase.status,
          totalCost: purchase.orderCost
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
    <div className="detail">
    <div className="App">
      <UserDetails user={user} purchases={purchases} />
      
    </div>
    </div>
  )
}

export default Detail;
