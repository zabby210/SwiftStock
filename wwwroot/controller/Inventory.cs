< !DOCTYPE html >
< html lang = "en" >
< head >
    < meta charset = "UTF-8" >
    < meta name = "viewport" content = "width=device-width, initial-scale=1.0" >
    < title > Inventory Management </ title >
    < style >
        /* General Styles */
        body {
    font - family: Arial, sans - serif;
display: flex;
margin: 0;
height: 100vh;
background: linear - gradient(to right, #FFD700, #FF0000);
        }

        /* Sidebar */
        .sidebar {
            width: 250px;
background: white;
color: #FF0000;
            padding: 20px;
display: flex;
flex - direction: column;
justify - content: space - between;
height: 100vh;
box - shadow: 2px 0px 10px rgba(0, 0, 0, 0.2);
        }

            .sidebar h2
{
    text-align: center;
    margin-bottom: 20px;
}

            .sidebar ul
{
    list-style: none;
    padding: 0;
    flex-grow: 1;
}

                .sidebar ul li {
                    margin: 10px 0;
                }

                    .sidebar ul li a {
                        text-decoration: none;
color: #FF0000;
                        display: block;
padding: 12px;
border - radius: 5px;
font - size: 18px;
text - align: center;
background: #FFD700;
                        font - weight: bold;
                    }

                        .sidebar ul li a:hover {
                            background: #FF0000;
                            color: white;
                        }

        /* Logout Button */
        .logout - btn {
background: #FF0000;
            color: white;
border: none;
padding: 12px;
cursor: pointer;
    border - radius: 5px;
    font - size: 18px;
width: 100 %;
    margin - top: auto;
    font - weight: bold;
}

            .logout - btn:hover {
                background: #FFD700;
                color: black;
            }

        /* Main Content */
        .main - content {
    flex - grow: 1;
padding: 30px;
display: flex;
    flex - direction: column;
    align - items: center;
}

        /* Inventory Container */
        .inventory - container {
background: white;
padding: 30px;
    border - radius: 12px;
    box - shadow: 0px 6px 12px rgba(0, 0, 0, 0.2);
width: 90 %;
    text - align: center;
}

h2 {
            color: #FF0000;
            font - size: 28px;
        }

        /* Search Bar */
        .search - container {
    margin - bottom: 15px;
}

# searchInput {
width: 60 %;
padding: 10px;
border: 2px solid #ddd;
            border-radius: 6px;
text - align: center;
font - size: 18px;
        }

        /* Inventory Table */
        table {
            width: 100 %;
margin - top: 15px;
border - collapse: collapse;
        }

        th, td {
            border: 1px solid #ddd;
            padding: 12px;
text - align: center;
font - size: 18px;
        }

        th {
            background: #FFD700;
            color: white;
font - size: 20px;
        }

        /* Buttons */
        .btn {
            background: #FFD700;
            color: white;
border: none;
padding: 14px;
cursor: pointer;
border - radius: 6px;
font - size: 16px;
font - weight: bold;
        }

            .btn: hover {
background: #FF0000;
            }

    </ style >
</ head >
< body >
    < div class= "sidebar" >
        < h2 > Inventory </ h2 >
        < ul >
            < li >< a href = "cashier.html" >🏠 Home </ a ></ li >
            < li >< a href = "inventory.html" >📦 Inventory </ a ></ li >
        </ ul >
        < button class= "logout-btn" onclick = "logout()" >🚪 Logout </ button >
    </ div >

    < div class= "main-content" >
        < div class= "inventory-container" >
            < h2 >📦 Inventory Management</h2>
            
            <!-- Search Bar -->
            <div class= "search-container" >
                < input type = "text" id = "searchInput" placeholder = "🔍 Search for a product..." onkeyup = "filterInventory()" >
            </ div >

            < table >
                < thead >
                    < tr >
                        < th > Product Name </ th >
                        < th > Barcode </ th >
                        < th > Price </ th >
                        < th > Stock </ th >
                    </ tr >
                </ thead >
                < tbody id = "inventoryTableBody" >
                    < !--Data will be loaded here -->
                </tbody>
            </table>
        </div>
    </div>

    <script>
        function logout() {
            fetch("http://localhost:5261/Account/Logout", {
                method: "POST",
                credentials: "include"
            })
                .then(response =>
                 {
                     if (!response.ok)
                     {
                         throw new Error("Logout request failed.");
                     }
                     return response.json();
                 })
                .then(data =>
                {
                    if (data.success)
                    {
                        localStorage.removeItem("userRole");
                        sessionStorage.clear();
                        window.location.href = "login.html";
                    }
                    else
                    {
                        alert("Logout failed! Try again.");
                    }
                })
                .catch(error =>
                {
                    console.error("Logout error:", error);
                });
        }

        async function loadInventory()
{
    try
    {
        const response = await fetch('/Inventory/GetInventory');
        const inventory = await response.json();
        const tableBody = document.getElementById('inventoryTableBody');
        tableBody.innerHTML = '';

        inventory.forEach(item =>
        {
            const row = `< tr >

                    < td >${ item.product_name}</ td >

                    < td >${ item.barcode}</ td >

                    < td >₱${ item.price.toFixed(2)}</ td >

                    < td >${ item.stock}</ td >

                </ tr >`;
            tableBody.innerHTML += row;
        });
    }
    catch (error)
    {
        console.error('Error loading inventory:', error);
    }
}

function filterInventory()
{
    let input = document.getElementById("searchInput").value.toLowerCase();
    let rows = document.getElementById("inventoryTableBody").getElementsByTagName("tr");

    for (let row of rows)
    {
        let productName = row.cells[0].textContent.toLowerCase();
        let barcode = row.cells[1].textContent.toLowerCase();
        row.style.display = productName.includes(input) || barcode.includes(input) ? "" : "none";
    }
}

loadInventory();
    </ script >
</ body >
</ html >
