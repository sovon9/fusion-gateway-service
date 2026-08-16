# fusion-gateway-service for Graphql Services
a fusion a master schema gateway for all your subgraphs


fusion subgraph pack -s ./subgraph/activity-service/schema.graphqls -w ./subgraph/activity-service

fusion subgraph pack -s ./subgraph/mes-service/schema.graphqls -w ./subgraph/mes-service



fusion compose -p ./supergraph/gateway -s ./subgraph/activity-service   

fusion compose -p ./supergraph/gateway -s ./subgraph/mes-service

fusion compose -p ./supergraph/gateway -s ./subgraph/process-order-service

fusion compose -p ./supergraph/gateway -s ./subgraph/product-service
