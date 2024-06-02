# Project 41: Ecommerce Website

### System components:
- Backend - ```sam-with-postgres```
- Frontend - ```frontend```
- StockManager - ```admin-page/vite-project```

**Deployment instructions are contained within each component's respective directory (usually in a readme)**

### Project management:
Project management was done primarily through GitHub issues, we also communicated through Messenger and Zoom.

### Project description:
This project is an e-commerce website that allows users to browse and purchase products. The website is built using React and the backend is built as an AWS lambda function API with a DynamoDB database. The website also has an admin page that allows the admin to manage the products in the database. Payment processing is done though Stripe and the backend state is update to ensure data consistency between the programs.

Authentication is done through AWS Cognito and the website is hosted on AWS S3. The stock manager enforces MFA on all accounts for added security. Endpoints related to stock management are seperated from the main API and in a fully locked controller to ensure that only the stock manager can access them.

### Deployed URLs:
- Ecommerce website: [EcogreenNZ](https://d3d9o3xxmxw7h7.cloudfront.net/)
- StockManager: [KashishWebManager](https://d3sowv95yo8bo1.cloudfront.net/)

## Future Plans
- The website could do with some more styling, some buttons need to provide more feedback to the user.
- The stock manager could be improved. The main stock management page works well but order tracking and customer information could be improved.
- The delivery system could be extended. Currently, it collects addresses and calculates delivery costs, but it is unable to generate delivery labels.
- Information could be cached on the frontend to reduce the number of API calls.
- Payment system could be extended to include more payment options (this is complete in principle but not deployment).
- The frontend could be extended to be more responsive to different screen sizes.

## Acknowledgements
- I would not like to thank AWS for their terrible documentation and virtually useless API specification.
- I would like to thank Stripe for their excellent documentation and the fact that they have a great API.






[![Review Assignment Due Date](https://classroom.github.com/assets/deadline-readme-button-24ddc0f5d75046c5622901739e7c5dd533143b0c8e959d652212380cedb1ea36.svg)](https://classroom.github.com/a/t8qno6SJ)


## Contributors
- George: 
  - Primary Backend developer 
  - General Frontend improvements 
  - Database 
  - Other AWS service integration 
  - Development Toolchain 
  - Authentication system 
  - Stripe integration
  
- Jason: 
  - Frontend primarily the main website.
- Mia: 
  - Frontend, primarily the admin-page but also the main website.
- CiCi: 
  - NZpost API system.
- James: 
  - PayPal system.
- Jesse:
 
