import React, {useState, useEffect, useContext} from 'react';
import UserDetails from '../Components/Usedetail/Usedetail';
import { ShopContext } from '../Context/ShopContext';
import Footer from '../Components/Footer/Footer';

function Detail() {
  const {serverUri} = useContext(ShopContext);
  const [user, setUser] = useState(null);
  const [purchases, setPurchases] = useState([]); // Initialize as an empty array
  const [error, setError] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const userId = sessionStorage.getItem('Id');
    const userUrl = serverUri + `/api/shop/GetCustomerByID/${userId}`;
    const purchasesUrl = serverUri + `/api/shop/GetCustomerOrders/${userId}`;

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
          packageReference: purchase.packageReference,
          date: purchase.createdAt
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
      <Footer />
    </div>
     
  )
}

export default Detail;
