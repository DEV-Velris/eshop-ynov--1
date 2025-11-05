using BuildingBlocks.Messaging.Events;
using Email.API.Services;
using MassTransit;
using System.Globalization;

namespace Email.API.Consumers;

/// <summary>
/// Consumer listening for OrderUpdatedEvent and sending order status update email
/// </summary>
public class OrderUpdatedEventConsumer(
    IEmailService emailService,
    ILogger<OrderUpdatedEventConsumer> logger) : IConsumer<OrderUpdatedEvent>
{
    public async Task Consume(ConsumeContext<OrderUpdatedEvent> context)
    {
        var orderEvent = context.Message;
        
        logger.LogInformation("📧 OrderUpdatedEvent received for order {OrderId} - Status changed to {OrderStatus}", 
            orderEvent.OrderId, orderEvent.OrderStatus);
        
        try
        {
            var subject = "Mise à jour du statut de votre commande";
            var body = BuildOrderStatusUpdateEmail(orderEvent);
            
            await emailService.SendEmailAsync(
                to: orderEvent.CustomerEmail,
                subject: subject,
                body: body
            );
            
            logger.LogInformation("✅ Order status update email sent to {Email}", orderEvent.CustomerEmail);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Failed to send order status update email for order {OrderId}", orderEvent.OrderId);
            throw;
        }
    }
    
    private static string BuildOrderStatusUpdateEmail(OrderUpdatedEvent orderEvent)
    {
        var frenchCulture = new CultureInfo("fr-FR");
        
        var (statusColor, statusIcon, statusMessage, statusDescription) = GetStatusInfo(orderEvent.OrderStatus);
        
        var itemsHtml = string.Join("", orderEvent.OrderItems.Select(item => 
            $@"<tr>
                <td>{item.ProductName}</td>
                <td style='text-align: center;'>{item.Quantity}</td>
                <td>{item.Price.ToString("C", frenchCulture)}</td>
                <td style='font-weight: 600;'>{(item.Price * item.Quantity).ToString("C", frenchCulture)}</td>
            </tr>"));

        return $@"
<html>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{ 
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; 
            margin: 0; 
            padding: 10px; 
            background-color: #f8f9fa; 
        }}
        .header {{ 
            background: linear-gradient(135deg, #2563eb, #1d4ed8); 
            color: white; 
            padding: 30px 20px; 
            text-align: center; 
            box-shadow: 0 2px 10px rgba(37, 99, 235, 0.2); 
        }}
        .header h1 {{ 
            margin: 0; 
            font-size: 28px; 
            font-weight: 600; 
        }}
        .container {{ 
            padding: 16px;
            max-width: 600px; 
            margin: 0 auto; 
            background-color: white; 
            border-radius: 12px; 
            overflow: hidden; 
            box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1); 
        }}
        .content {{ 
            padding: 30px; 
        }}
        .greeting {{ 
            font-size: 20px; 
            color: #1f2937; 
            margin-bottom: 20px; 
            font-weight: 500; 
        }}
        .status-banner {{
            padding: 20px;
            border-radius: 12px;
            margin: 20px 0;
            text-align: center;
            font-size: 18px;
            font-weight: 600;
            color: white;
            background-color: {statusColor};
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
        }}
        .status-icon {{
            font-size: 24px;
            margin-bottom: 8px;
        }}
        .order-info {{ 
            background-color: #f1f5f9; 
            border-radius: 8px; 
            padding: 20px; 
            margin: 20px 0; 
            border-left: 4px solid #2563eb; 
        }}
        .summary-table {{ 
            width: 100%; 
            border-collapse: collapse; 
            margin: 20px 0; 
            background-color: white; 
            border-radius: 8px; 
            overflow: hidden; 
            box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1); 
        }}
        .summary-table th {{ 
            background-color: #2563eb; 
            color: white; 
            padding: 15px; 
            text-align: left; 
            font-weight: 600; 
        }}
        .summary-table td {{ 
            padding: 12px 15px; 
            border-bottom: 1px solid #e5e7eb; 
        }}
        .items-table {{ 
            width: 100%; 
            border-collapse: collapse; 
            margin: 20px 0; 
            background-color: white; 
            border-radius: 8px; 
            overflow: hidden; 
            box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1); 
        }}
        .items-table th {{ 
            background-color: #475569; 
            color: white; 
            padding: 12px; 
            text-align: left; 
            font-weight: 600; 
        }}
        .items-table td {{ 
            padding: 12px; 
            border-bottom: 1px solid #e5e7eb; 
        }}
        .items-table tr:nth-child(even) {{ 
            background-color: #f8fafc; 
        }}
        .footer {{ 
            text-align: center; 
            color: #6b7280; 
            margin-top: 30px; 
            padding-top: 20px; 
            border-top: 1px solid #e5e7eb; 
            font-size: 14px; 
        }}
        .previous-status {{
            color: #6b7280;
            font-size: 14px;
            font-style: italic;
            margin-top: 8px;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>📦 Mise à jour de commande</h1>
        </div>
        
        <div class='content'>
            <div class='greeting'>
                Bonjour <strong>{orderEvent.CustomerName}</strong> ! 👋
            </div>
            
            <p>Le statut de votre commande <strong>#{orderEvent.OrderId}</strong> a été mis à jour.</p>
            
            <div class='status-banner'>
                <div class='status-icon'>{statusIcon}</div>
                <div>{statusMessage}</div>
            </div>
            
            <p>{statusDescription}</p>
            
            <div class='order-info'>
                <h3 style='margin-top: 0; color: #2563eb;'>📋 Informations de la commande</h3>
                <table class='summary-table'>
                    <tr>
                        <th>Information</th>
                        <th>Détails</th>
                    </tr>
                    <tr>
                        <td><strong>ID Commande</strong></td>
                        <td>{orderEvent.OrderId}</td>
                    </tr>
                    <tr>
                        <td><strong>Date de mise à jour</strong></td>
                        <td>{orderEvent.UpdatedDate:dd/MM/yyyy à HH:mm}</td>
                    </tr>
                    <tr>
                        <td><strong>Statut actuel</strong></td>
                        <td style='color: {statusColor}; font-weight: 600;'>{GetFrenchStatusName(orderEvent.OrderStatus)}</td>
                    </tr>
                    <tr>
                        <td><strong>Montant total</strong></td>
                        <td style='font-weight: 600; color: #059669; font-size: 16px;'>{orderEvent.TotalPrice.ToString("C", frenchCulture)}</td>
                    </tr>
                </table>
            </div>
            
            <h3 style='color: #2563eb; margin-top: 30px;'>🛍️ Articles de la commande</h3>
            <table class='items-table'>
                <thead>
                    <tr>
                        <th>Produit</th>
                        <th>Quantité</th>
                        <th>Prix unitaire</th>
                        <th>Total</th>
                    </tr>
                </thead>
                <tbody>
                    {itemsHtml}
                </tbody>
            </table>
            
            <div class='footer'>
                <p><em>Ceci est un email automatique généré par notre système e-commerce.</em></p>
                <p>Si vous avez des questions, n'hésitez pas à nous contacter.</p>
            </div>
        </div>
    </div>
</body>
</html>";
    }
    
    private static (string color, string icon, string message, string description) GetStatusInfo(string status)
    {
        return status.ToLower() switch
        {
            "draft" => ("#94a3b8", "📝", "Commande en brouillon", "Votre commande est en cours de préparation."),
            "pending" => ("#f59e0b", "⏳", "Commande en attente", "Votre commande est en cours de traitement."),
            "submitted" => ("#3b82f6", "📤", "Commande soumise", "Votre commande a été soumise et est en cours de validation."),
            "confirmed" => ("#10b981", "✅", "Commande confirmée", "Excellente nouvelle ! Votre commande a été confirmée et est en cours de préparation."),
            "shipped" => ("#8b5cf6", "🚚", "Commande expédiée", "Votre commande est en route ! Vous devriez la recevoir sous peu."),
            "delivered" => ("#059669", "📦", "Commande livrée", "Parfait ! Votre commande a été livrée avec succès."),
            "completed" => ("#16a34a", "🎉", "Commande terminée", "Votre commande est maintenant terminée. Merci pour votre achat !"),
            "cancelled" => ("#ef4444", "", "Commande annulée", "Votre commande a été annulée. Si vous avez des questions, contactez notre service client."),
            _ => ("#6b7280", "ℹ️", $"Statut : {GetFrenchStatusName(status)}", "Le statut de votre commande a été mis à jour.")
        };
    }
    
    private static string GetFrenchStatusName(string status)
    {
        return status.ToLower() switch
        {
            "draft" => "Brouillon",
            "pending" => "En attente",
            "submitted" => "Soumise",
            "confirmed" => "Confirmée",
            "shipped" => "Expédiée",
            "delivered" => "Livrée",
            "completed" => "Terminée",
            "cancelled" => "Annulée",
            _ => status
        };
    }
}